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
using System.Windows.Forms;

namespace CleanMyPhone
{
    public class Program
    {
        private static Mutex singleInstanceMutex;

        [STAThread]
        static void Main(string[] args)
        {
            ExitAppIfAnotherProcessIsRunning("CleanMyPhone");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var mainForm = new Main();
            if (args.Any(x => x == "-hidden"))
            {
                mainForm.WindowState = FormWindowState.Minimized;
                mainForm.ShowInTaskbar = false;
            }

            Application.Run(mainForm);
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
}
