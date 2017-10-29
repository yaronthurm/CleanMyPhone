using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CleanMyPhone
{
    public class SingleDevicePhoneCleaner
    {
        public event Action<SingleDevicePhoneCleaner, string> NewLogLineAdded;

        public string _deviceID;
        private CleanerSettings _deviceSettings;

        private string _logFilePath;
        private string _rollingLogPath;
        private int _rollingLogLength;

        private EmailSender _emailSender;
        private SummaryData _summary = new SummaryData();
        private IFileManager _sourceFileManager;
        private BasicFileInfo[] _sourceFiles;
        private Thread _thread;
        private CancellationTokenSource _cancelToken = new CancellationTokenSource();
        

        public SingleDevicePhoneCleaner(string deviceID, CleanerSettings deviceSettings)
        {
            this._deviceID = deviceID;
            this._deviceSettings = deviceSettings;
        }

        public void RunInBackground()
        {
            _thread = new Thread(Run);
            _thread.IsBackground = true;
            _thread.Start();
        }

        public void Cancel()
        {
            _cancelToken.Cancel();
        }

        public void WaitForIdle()
        {
            _thread.Join();
        }


        private void Run()
        {
            while (!_cancelToken.IsCancellationRequested)
            {
                try
                {
                    CreateLogFile();
                    PrintConfigValues();
                    WaitForSourceToBeAvailable();
                    InitializeEmailSender();

                    MarkStartTime();
                    InitializeSourceFilesList();
                    CopyMissingFiles();
                    if (EnableDeleting())
                    {
                        var filesToDelete = GetListOfFilesToDelete(_deviceSettings.HighMbThreshold, _deviceSettings.LowMbThreshold, GetExcludeList());
                        if (filesToDelete.Any())
                            DeleteFilesForCleanup(filesToDelete);
                    }
                    MarkEndTime();
                    TrySendSuccessRunEmail();
                }
                catch (OperationCanceledException)
                {
                    WriteToConsoleAndToLog("Operation was canceled");
                }
                catch (Exception ex)
                {
                    MarkEndTime();
                    WriteToConsoleAndToLog("An error has occured: " + ex.ToString());
                    TrySendFailedRunEmail(ex);
                }
                finally
                {
                    WaitForIdleTimeToPass();
                }
            }
        }

        private bool EnableDeleting()
        {
            return _deviceSettings.EnableDeleting;
        }

        private void TrySendFailedRunEmail(Exception ex)
        {
            if (_emailSender == null) return;

            try
            {
                var subject = "Phone Cleanup Summary - Failed";
                var body = $"There was an error during the last run:\n" +
                    $"Start time: {_summary.StartTime.ToLocalTime()}\n" +
                    $"Duration: {_summary.EndTime - _summary.StartTime}\n" +
                    $"Total missing files copied: {_summary.TotalMissingFilesCopied}\n" +
                    $"Total Mb Found in phone: {_summary.TotalMbBeforeDelete:0.##}[MB]\n" +
                    $"Threashold for deletion: {_summary.HighMBThreshold}[MB]\n" +
                    $"Threashold for stop deletion: {_summary.LowMBThreshold}[MB]\n" +
                    $"Total files that were deleted: {_summary.TotalFilesDeleted}\n" +
                    $"Total Mb deleted: {_summary.TotalMbDeleted:0.##}[MB]\n" +
                    $"Log file: {_logFilePath}\n\n\n" +
                    $"Error: {ex.ToString()}\n";

                _emailSender.SendEmail("PhoneCleaner", subject, body);
            }
            catch (Exception e)
            {
                WriteToConsoleAndToLog("Failed to send email. " + e.Message);
            }
        }

        private void InitializeSourceFilesList()
        {
            _cancelToken.Token.ThrowIfCancellationRequested();
            WriteToConsoleAndToLog("Retrieving files from source");
            _sourceFiles = _sourceFileManager.ListFiles(_deviceSettings.SourceFolder).ToArray();
            var totalSizeMB = BytesToMegabytes(_sourceFiles.Select(x => x.SizeInBytes).Sum());
            _summary.TotalMbBeforeDelete = totalSizeMB;
            WriteToConsoleAndToLog($"Found {_sourceFiles.Length} Files. Total size: {totalSizeMB:0.##[MB]}");
        }



        private string FindWebDavServerIP(int port)
        {
            while (true)
            {
                _cancelToken.Token.ThrowIfCancellationRequested();
                WriteToRollingLog($"Searching for active servers");
                var subnets = GetAllSubnets();
                WriteToRollingLog($"Found subnets: '{string.Join(",", subnets)}'");
                var activeIPsOnThatPort = subnets.SelectMany(x => FindActiveIPsInNetwork(x, port)).ToArray();
                WriteToRollingLog($"Found {activeIPsOnThatPort.Length} active servers listening on port '{port}': [{string.Join(",", activeIPsOnThatPort)}]");
                foreach (var ip in activeIPsOnThatPort)
                {
                    var client = new WebDavFileManager(ip, port, _deviceSettings.Username, _deviceSettings.Password);
                    var guidFileName = "/Cleaner/guid.txt";
                    if (client.Exist(guidFileName))
                    {
                        WriteToRollingLog($"Found guid file on server '{ip}'");
                        var content = client.GetFileContentAsString(guidFileName);
                        var deviceGuid = DeviceGuid.LoadFromContent(content);
                        WriteToRollingLog($"guid file on server '{ip}' contains the following content: {deviceGuid.ID}, expected value: {_deviceID}");
                        if (deviceGuid.ID == _deviceID)
                        {
                            WriteToRollingLog($"Found match for deviceID '{_deviceID}' on server '{ip}'");
                            return ip;
                        }
                        else
                        {
                            WriteToRollingLog($"Did not find match for deviceID '{_deviceID}' on server '{ip}'");
                        }
                    }
                    else
                    {
                        WriteToRollingLog($"Did not find guid file on server '{ip}'");
                    }
                }

                Sleep(TimeSpan.FromSeconds(20));
            }
        }

        private void Sleep(TimeSpan timeSpan)
        {
            var timeToWakeup = DateTime.Now.Add(timeSpan);
            while (DateTime.Now < timeToWakeup)
            {
                if (_cancelToken.Token.IsCancellationRequested) return; 

                var millisecondsLeft = (timeToWakeup - DateTime.Now).TotalMilliseconds;
                var millisecondsToSleep = (int)Math.Min(1000, millisecondsLeft);
                WriteToRollingLog($"Sleeping: {millisecondsToSleep}[ms]. Time till wakeup: {millisecondsLeft}[ms]");
                Thread.Sleep(millisecondsToSleep);
            }
        }

        private List<string> FindActiveIPsInNetwork(string subnet, int port)
        {
            var ips = Enumerable.Range(0, 255).Select(x => $"{subnet}.{x}");

            var bag = new ConcurrentBag<string>();
            Parallel.ForEach(ips, ip => {
                if (_cancelToken.IsCancellationRequested) return;
                var tcpClient = new TcpClient();
                var connectTask = tcpClient.ConnectAsync(ip, port);
                var timeoutTask = Task.Delay(200);
                var completedTaskIndex = Task.WaitAny(connectTask, timeoutTask);
                if (completedTaskIndex == 0)
                    bag.Add(ip);
            });

            return bag.ToList();
        }

        private void MarkEndTime()
        {
            _summary.EndTime = DateTime.UtcNow;
        }

        private void MarkStartTime()
        {
            _summary.StartTime = DateTime.UtcNow;
        }

        private void PrintConfigValues()
        {
            WriteToConsoleAndToLog($"Loaded configuration from: {_deviceSettings.SettingsFile}");
            WriteToConsoleAndToLog($"\tSourceFolder: {_deviceSettings.SourceFolder}");
            WriteToConsoleAndToLog($"\tDestinationFolder: {_deviceSettings.DestinationFolder}");
            WriteToConsoleAndToLog($"\tDeviceFolder: {_deviceSettings.DeviceFolder}");
            WriteToConsoleAndToLog($"\tEnableDeleting: {_deviceSettings.EnableDeleting}");
            if (_deviceSettings.EnableDeleting)
            {
                WriteToConsoleAndToLog($"\tHighMBThreshold: {_deviceSettings.HighMbThreshold}");
                WriteToConsoleAndToLog($"\tLowMBThreshold: {_deviceSettings.LowMbThreshold}");
            }
        }

        private void WaitForIdleTimeToPass()
        {
            if (_cancelToken.Token.IsCancellationRequested) return;
            WriteToConsoleAndToLog($"Waiting for idle time to pass: {_deviceSettings.IdleTimeBetweenRunsInSeconds}[sec]");
            Sleep(TimeSpan.FromSeconds(_deviceSettings.IdleTimeBetweenRunsInSeconds));
        }

        private void WaitForSourceToBeAvailable()
        {
            WriteToConsoleAndToLog("Waiting for source to become available");
            var serverIP = FindWebDavServerIP(_deviceSettings.Port);
            _sourceFileManager = new WebDavFileManager(serverIP, _deviceSettings.Port, _deviceSettings.Username, _deviceSettings.Password);
            WriteToConsoleAndToLog($"Source is now available on: http://{serverIP}:{_deviceSettings.Port}/");
        }

        private string[] GetAllSubnets()
        {
            var ret = Dns.GetHostAddresses(Dns.GetHostName())
                .Where(x => x.AddressFamily == AddressFamily.InterNetwork)
                .Select(x => x.ToString().Split('.'))
                .Select(x => $"{x[0]}.{x[1]}.{x[2]}"); // Assume subnet of 255.255.255.0
            return ret.ToArray();
        }

        private void TrySendSuccessRunEmail()
        {
            if (_emailSender == null) return;

            try
            {
                var subject = "Phone Cleanup Summary";
                var body = $"Cleanup summary:\n" +
                    $"Start time: {_summary.StartTime.ToLocalTime()}\n" +
                    $"Duration: {_summary.EndTime - _summary.StartTime}\n" +
                    $"Total missing files copied: {_summary.TotalMissingFilesCopied}\n" +
                    $"Total Mb Found in phone: {_summary.TotalMbBeforeDelete:0.##}[MB]\n" +
                    $"Threashold for deletion: {_summary.HighMBThreshold}[MB]\n" +
                    $"Threashold for stop deletion: {_summary.LowMBThreshold}[MB]\n" +
                    $"Total files that were deleted: {_summary.TotalFilesDeleted}\n" +
                    $"Total Mb deleted: {_summary.TotalMbDeleted:0.##}[MB]\n" +
                    $"Log file: {_logFilePath}\n";
                _emailSender.SendEmail("PhoneCleaner", subject, body);
            }
            catch (Exception ex)
            {
                WriteToConsoleAndToLog("Failed to send email. " + ex.Message);
            }
        }

        private void InitializeEmailSender()
        {
            _summary = new SummaryData();
            var smtpConfigPath = Path.Combine(_deviceSettings.DeviceFolder, "smtp.txt");
            if (!File.Exists(smtpConfigPath))
                return;
            else
                _emailSender = EmailSender.GetEmailSender(smtpConfigPath);
        }

        private void CreateLogFile()
        {
            var logFolder = Path.Combine(_deviceSettings.DeviceFolder, "Logs");
            var logName = DateTime.Now.ToString("yyyy_MM_dd  HH_mm_ss") + ".log";
            var ret = Path.Combine(logFolder, logName);
            if (!Directory.Exists(logFolder))
                Directory.CreateDirectory(logFolder);
            File.WriteAllText(ret, "");

            _logFilePath = ret;

            _rollingLogPath = Path.Combine(_deviceSettings.DeviceFolder, "Logs", "rolling.log");
            if (!File.Exists(_rollingLogPath))
                File.WriteAllText(_rollingLogPath, "");
        }

        private List<string> GetExcludeList()
        {
            var filename = Path.Combine(_deviceSettings.DeviceFolder, "ExcludeFromCleanup.txt");
            if (!File.Exists(filename))
                return new List<string>();
            var ret = File.ReadAllLines(filename)
                .Select(x => x.Trim())
                .ToList();
            return ret;
        }

        private void DeleteFilesForCleanup(List<BasicFileInfo> filesToDelete)
        {
            WriteToConsoleAndToLog($"About to delete {filesToDelete.Count} files to clean up storage");
            var doneCount = 0;
            foreach (var file in filesToDelete)
            {
                _cancelToken.Token.ThrowIfCancellationRequested();
                WriteToConsoleAndToLog($"Deleting file {++doneCount}/{filesToDelete.Count}: {file.FullName}");
                var fileSize = file.SizeInBytes;
                _sourceFileManager.Delete(file);

                _summary.TotalMbDeleted += BytesToMegabytes(fileSize);
                _summary.TotalFilesDeleted++;
            }
        }

        private void CopyMissingFiles()
        {
            WriteToConsoleAndToLog($"Copy Missing Files. Source: {_deviceSettings.SourceFolder}, Destination: {_deviceSettings.DestinationFolder}");

            var destinationFilesNames = Directory.GetFiles(_deviceSettings.DestinationFolder).Select(Path.GetFileName).ToLookup(x => x);
            var missingFiles = _sourceFiles.Where(x => !destinationFilesNames.Contains(x.Name)).ToArray();

            WriteToConsoleAndToLog($"Found {missingFiles.Length} missing files.");

            var doneCount = 0;
            if (missingFiles.Any())
            {
                var tmpFolder = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
                Directory.CreateDirectory(tmpFolder);
                foreach (var missingFile in missingFiles)
                {
                    _cancelToken.Token.ThrowIfCancellationRequested();
                    WriteToConsoleAndToLog($"Copying missing file {++doneCount}/{missingFiles.Length}: {missingFile.Name}");
                    var tmpPath = Path.Combine(tmpFolder, missingFile.Name);
                    _sourceFileManager.CopyWithTimestamps(missingFile, tmpPath);

                    // Check file length
                    var tmpFile = new FileInfo(tmpPath);
                    if (tmpFile.Length == missingFile.SizeInBytes) // OK
                    {
                        var destinationPath = Path.Combine(_deviceSettings.DestinationFolder, missingFile.Name);
                        File.Move(tmpPath, destinationPath);
                        File.SetCreationTimeUtc(destinationPath, tmpFile.CreationTimeUtc);
                        File.SetLastAccessTimeUtc(destinationPath, tmpFile.LastAccessTimeUtc);
                        File.SetLastWriteTimeUtc(destinationPath, tmpFile.LastWriteTimeUtc);
                        _summary.TotalMissingFilesCopied++;
                    }
                }
                Directory.Delete(tmpFolder);
            }
        }

        private List<BasicFileInfo> GetListOfFilesToDelete(int highMBThreshold, int lowMBThreshold, List<string> excludeFilesShortName)
        {
            WriteToConsoleAndToLog($"Look for files to delete. Start delete threshold: {highMBThreshold}[MB], Stop delete threshold: {lowMBThreshold}[MB], exclude files count: {excludeFilesShortName.Count}");
            var ret = new List<BasicFileInfo>();
            var totalSizeMB = BytesToMegabytes(_sourceFiles.Select(x => x.SizeInBytes).Sum());

            _summary.HighMBThreshold = highMBThreshold;
            _summary.LowMBThreshold = lowMBThreshold;
            if (totalSizeMB > highMBThreshold)
            {
                var amountToRemoveMB = totalSizeMB - lowMBThreshold;
                WriteToConsoleAndToLog($"Total size {totalSizeMB:0.##}[MB] exceeds threshold of {highMBThreshold}[MB]. Need to delete {amountToRemoveMB:0.##}[MB]");

                // Filter out files that should not be removed
                var validSourceFilesForDeletion = _sourceFiles
                    .Where(x => x.LastWriteTimeUtc < DateTime.UtcNow.AddMonths(-1)) // For cases where the 'created' attribute is missing (e.g. when mapping a drive over WebDAV)
                    .Where(x => !excludeFilesShortName.Contains(x.Name)) // Respect exclude files list
                    .ToArray();
                WriteToConsoleAndToLog($"Found {validSourceFilesForDeletion.Length} valid files for deletion. Total size: {BytesToMegabytes(validSourceFilesForDeletion.Select(x => x.SizeInBytes).Sum()):0.##}[MB]");
                Shuffel(validSourceFilesForDeletion);

                decimal currentAmountMB = 0;
                foreach (var file in validSourceFilesForDeletion)
                {
                    ret.Add(file);
                    currentAmountMB += BytesToMegabytes(file.SizeInBytes);
                    if (currentAmountMB >= amountToRemoveMB)
                        break;
                }
            }

            WriteToConsoleAndToLog($"Selected {ret.Count} files for deletion. Total size: {BytesToMegabytes(ret.Select(x => x.SizeInBytes).Sum()):0.##}[MB]");
            return ret;
        }

        private static decimal BytesToMegabytes(long bytes)
        {
            return ((decimal)bytes) / 1024 / 1024;
        }

        private static void Shuffel<T>(T[] sourceFiles)
        {
            var random = new Random();
            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < sourceFiles.Length; i++)
                {
                    var swapWith = random.Next(sourceFiles.Length);
                    var tmp = sourceFiles[swapWith];
                    sourceFiles[swapWith] = sourceFiles[i];
                    sourceFiles[i] = tmp;
                }
            }
        }

        private void WriteToConsoleAndToLog(string text)
        {
            WriteToRollingLog(text);
            var textWithTime = $"[{DateTime.Now}]  {text}";
            Console.WriteLine(textWithTime);
            File.AppendAllText(_logFilePath, textWithTime + Environment.NewLine);
        }

        private void WriteToRollingLog(string text)
        {
            if (_rollingLogLength == 0)
                _rollingLogLength = File.ReadLines(_rollingLogPath).Count();

            if (_rollingLogLength > 1000)
            {
                var trailingLines = File.ReadLines(_rollingLogPath).Skip(500).ToArray();
                File.WriteAllLines(_rollingLogPath, trailingLines);
                _rollingLogLength = trailingLines.Length;
            }

            var textWithTime = $"[{DateTime.Now}]  {text}";
            File.AppendAllText(_rollingLogPath, textWithTime + Environment.NewLine);
            _rollingLogLength++;
            this.NewLogLineAdded?.Invoke(this, textWithTime);
        }
    }


    public class SummaryData
    {
        public DateTime StartTime;
        public DateTime EndTime;
        public int TotalMissingFilesCopied;
        public int TotalFilesDeleted;
        public decimal TotalMbDeleted;
        public decimal TotalMbBeforeDelete;
        public int HighMBThreshold;
        public int LowMBThreshold;
    }
}
