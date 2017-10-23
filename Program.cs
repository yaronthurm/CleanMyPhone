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

            if (args.Any(x => x.StartsWith("-deviceID=")))
            {
                var deviceID = args.First(x => x.StartsWith("-deviceID=")).Replace("-deviceID=", "");
                ExitAppIfAnotherProcessIsRunning(deviceID);

                var deviceSettings = CleanerSettings.GetAllConfigs(appFolder)[deviceID];
                var cleaner = new SingleDevicePhoneCleaner(deviceID, deviceSettings);
                if (!isHidden)
                    Console.Title = $"Cleaner-{deviceID}";
                cleaner.Run();
            }
            else {
                var cleaner = new MultipleDevicesCleaner(appFolder, isHidden);
                cleaner.Run();
            }
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
