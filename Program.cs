using CleanMyPhone;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace CleanMyPhone
{
    public class Program
    {
        private static Mutex singleInstanceMutex;

        static void Main(string[] args)
        {
            var isHidden = args.Any(x => x == "-hidden");
            if (!isHidden)
                AllocConsole();

            var appFolder = ConfigurationManager.AppSettings["AppFolder"];

            if (args.Any(x => x.StartsWith("-setup")) && !isHidden)
            {
                StartSetupFlow(appFolder);
            }
            else if (args.Any(x => x.StartsWith("-deviceID=")))
            {
                StartDpecificDeviceFlow(args, appFolder, isHidden);
            }
            else {
                StartMultipleDevicesFlow(appFolder, isHidden);
            }
        }

        private static void StartSetupFlow(string appFolder)
        {
            string deviceID = ReadFromConsole(
                "Enter desired decive ID (Valid charecters: a-z,A-Z,0-9,'_','-',' ', up to 100 charecters)",
                "Invalid devide ID",
                IsDeviceIDValid,
                x => x);

            string ip = ReadFromConsole("Enter device IP", "Invalid IP format", IsValidIP, x => IPAddress.Parse(x).ToString());
            int port = ReadFromConsole("Enter port", "Invalid port (1-65535)", x => IsValidInt(x, 1, 65535), int.Parse);
            string username = ReadFromConsole("Enter username", "", x => true, x => x);
            string password = ReadFromConsole("Enter password", "", x => true, x => x);
            string sourceFolder = ReadFromConsole("Enter source folder (usually DCIM/Camera)", "", x => true, x => x);
            string destinationFolder = ReadFromConsole("Enter destination folder", "", x => true, x => x);
            int highThreshold = ReadFromConsole("Enter High threshod [MB] (10,000-50,000)", "Invalid value", x => IsValidInt(x, 10000, 50000), int.Parse);
            int lowThreshold = ReadFromConsole("Enter Low threshold [MB] (10,000-50,000)", "Invalid value", x => IsValidInt(x, 10000, 50000), int.Parse);
            int idleTimeInSeconds = ReadFromConsole("Enter Idle time in seconds (180-1800)", "Invalid value", x => IsValidInt(x, 180, 1800), int.Parse);

            DeviceSetup.SetupDevice(appFolder, deviceID, ip, port, username, password, sourceFolder, destinationFolder, highThreshold, lowThreshold, idleTimeInSeconds);
        }

        private static T ReadFromConsole<T>(string text, string errorText, Func<string, bool> checkInput, Func<string, T> selector)
        {
            while (true)
            {
                Console.WriteLine(text);
                var input = Console.ReadLine();
                if (!checkInput(input))
                {
                    Console.WriteLine(errorText);
                }
                else {
                    var ret = selector(input);
                    return ret;
                }
            }
        }

        private static bool IsValidIP(string ip)
        {
            IPAddress tmp;
            var ret = IPAddress.TryParse(ip, out tmp);
            return ret;
        }

        private static bool IsValidInt(string valueString, int min, int max)
        {
            int value;
            if (!int.TryParse(valueString, out value))
                return false;
            if (value < min || value > max)
                return false;
            return true;
              
        }

        private static bool DeviceIDAlreadyExists(string deviceID, string appFolder)
        {
            var deviceSettings = CleanerSettings.GetAllConfigs(appFolder);
            return deviceSettings.ContainsKey(deviceID);
        }

        private static bool IsDeviceIDValid(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || id.Length > 100)
                return false;
            foreach (var c in id)
            {
                if (!(c >= '0' && c <= '9') &&
                    !(c >= 'a' && c <= 'z') &&
                    !(c >= 'A' && c <= 'Z') &&
                    !(c == '-' || c == '_' || c == ' '))

                    return false;
            }
            return true;
        }

        private static void StartMultipleDevicesFlow(string appFolder, bool isHidden)
        {
            var cleaner = new MultipleDevicesCleaner(appFolder, isHidden);
            cleaner.Run();
        }

        private static void StartDpecificDeviceFlow(string[] args, string appFolder, bool isHidden)
        {
            var deviceID = args.First(x => x.StartsWith("-deviceID=")).Replace("-deviceID=", "");
            ExitAppIfAnotherProcessIsRunning(deviceID);

            var deviceSettings = CleanerSettings.GetAllConfigs(appFolder)[deviceID];
            var cleaner = new SingleDevicePhoneCleaner(deviceID, deviceSettings);
            if (!isHidden)
                Console.Title = $"Cleaner-{deviceID}";
            cleaner.Run();
        }

        private static void ExitAppIfAnotherProcessIsRunning(string deviceID)
        {
            Console.WriteLine($"Check for another process: trying to lock on {deviceID}");
            singleInstanceMutex = new Mutex(true, deviceID);
            var mutexAcquired = singleInstanceMutex.WaitOne(TimeSpan.Zero, true);

            if (!mutexAcquired) {  // Meaning it is locked by another process
                Console.WriteLine("Another process is already running");
                Thread.Sleep(3000);
                Environment.Exit(-1);
            }
        }



        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
    }



    public class MultipleDevicesCleaner
    {
        private List<Process> _devicesProcesses = new List<Process>();
        private Dictionary<string, CleanerSettings> _devicesSettings;
        private string _appFolder;
        private bool _isHidden;

        public MultipleDevicesCleaner(string appFolder, bool isHidden)
        {
            _appFolder = appFolder;
            _isHidden = isHidden;
        }

        public void Run()
        {
            LoadConfig();
            DispatchProcessForEachDevice();
            WaitForAllProcessedToFinish();
        }

        private void WaitForAllProcessedToFinish()
        {
            Console.WriteLine($"Waiting for dispathed processed to exit");
            foreach (var process in _devicesProcesses)
                process.WaitForExit();
        }

        private void LoadConfig()
        {
            _devicesSettings = CleanerSettings.GetAllConfigs(_appFolder);
        }

        private void DispatchProcessForEachDevice()
        {
            var exeFile = Assembly.GetEntryAssembly().Location;
            foreach (var deviceID in _devicesSettings.Keys)
            {
                var arguments = _isHidden ? $"-deviceID={deviceID} -hidden" : $"-deviceID={deviceID}";
                Process process  = Process.Start(exeFile, arguments);
                _devicesProcesses.Add(process);
                Console.WriteLine($"Dispathed process for device '{deviceID}': {exeFile} {arguments}");
            }
        }
    }
}
