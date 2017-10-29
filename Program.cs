﻿using CleanMyPhone;
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
using System.Windows.Forms;

namespace CleanMyPhone
{
    public class Program
    {
        private static Mutex singleInstanceMutex;

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var mainForm = new Main();
            var isHidden = args.Any(x => x == "-hidden");

            if (isHidden)
                mainForm.WindowState = FormWindowState.Minimized;

            Application.Run(mainForm);
            return;

            if (!isHidden)
                AllocConsole();

            var appFolder = GetAppFolder();

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

        private static string GetAppFolder()
        {
            var pathToAppFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "CleanMyPhone");
            if (!Directory.Exists(pathToAppFolder))
                Directory.CreateDirectory(pathToAppFolder);
            return pathToAppFolder;
        }

        private static void StartSetupFlow(string appFolder)
        {
            DeviceSetup.ReadFromConsoleAndSetupDevice(appFolder);
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
            //cleaner.Run();
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
