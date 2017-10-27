using CleanMyPhone;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            string deviceID;
            while (true)
            {
                Console.WriteLine("Enter desired decive ID (Valid charecters: a-z,A-Z,0-9,'_','-',' ', up to 100 charecters)");
                deviceID = Console.ReadLine();
                if (!IsDeviceIDValid(deviceID))
                {
                    Console.WriteLine("Device ID is invalid");
                }
                else if (DeviceIDAlreadyExists(deviceID, appFolder))
                {
                    Console.WriteLine("Device ID already exist");
                }
                else
                    break;
            }

            string ip;
            Console.WriteLine("Enter decive IP");
            ip = Console.ReadLine();

            string port;
            Console.WriteLine("Enter port");
            port = Console.ReadLine();

            string username;
            Console.WriteLine("Enter username");
            username = Console.ReadLine();

            string password;
            Console.WriteLine("Enter password");
            password = Console.ReadLine();

            string sourceFolder;
            Console.WriteLine("Enter source folder (usually DCIM/Camera)");
            sourceFolder = Console.ReadLine();

            string destinationFolder;
            Console.WriteLine("Enter destination folder");
            destinationFolder = Console.ReadLine();

            string highThreshold;
            Console.WriteLine("Enter High threshod [MB]");
            highThreshold = Console.ReadLine();

            string lowThreshold;
            Console.WriteLine("Enter Low threshold [MB]");
            lowThreshold = Console.ReadLine();

            string idleTimeInSeconds;
            Console.WriteLine("Enter Idle time in seconds (e.g. 180)");
            idleTimeInSeconds = Console.ReadLine();


            DeviceSetup.SetupDevice(appFolder, deviceID, ip, port, username, password, sourceFolder, destinationFolder, highThreshold, lowThreshold, idleTimeInSeconds);
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
