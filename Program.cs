using CleanMyPhone;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
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


        private static void ExtractDataFromLogs()
        {
            var pathToDevicesFolder = "";
            var device = "";
            var targetFolder = "";

            //var data = File.ReadLines($@"{pathToDevicesFolder}\{device}\FilesActivity.txt")
            //    .Select(JObject.Parse)
            //    .GroupBy(x => x["file_name"].ToString() + "_" + x["operation"].ToString())
            //    .Select(x => x.First())
            //    .OrderBy(x => x["current_time"])
            //    .Select(x => x.ToString(Newtonsoft.Json.Formatting.None))
            //    .ToArray();

            //File.WriteAllLines($@"{pathToDevicesFolder}\{device}\FilesActivity1.txt", data);
            //return;

            
            foreach (var file in Directory.GetFiles($@"{pathToDevicesFolder}\{device}\Logs"))
            {
                if (file == $@"{pathToDevicesFolder}\{device}\Logs\rolling.log")
                    continue;
                var copied = File.ReadLines(file)
                    .Where(x => x.Contains("Copying missing file"))
                    .Select(x => x.Split(new[] { "  Copying missing file" }, StringSplitOptions.None))
                    .Select(x => new {
                        activityStart = DateTime.ParseExact(Path.GetFileNameWithoutExtension(file), "yyyy_MM_dd  HH_mm_ss", CultureInfo.InvariantCulture),
                        time = DateTime.ParseExact(x[0].Trim('[', ']'), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                        filename = x[1].Split(':')[1].Trim()
                    })
                    .Select(x => new
                    {
                        main_activity_start_time = x.activityStart,
                        current_time = x.time,
                        operation = "file-copied",
                        file_name = x.filename,
                        path_to_copy = Path.Combine(targetFolder, x.filename)
                    })
                    .Select(Newtonsoft.Json.JsonConvert.SerializeObject)
                    .ToArray();
                if (copied.Any())
                {
                    File.AppendAllLines($@"{pathToDevicesFolder}\{device}\FilesActivity.txt", copied);
                }


                var deleted = File.ReadLines(file)
                    .Where(x => x.Contains("Deleting file"))
                    .Select(x => x.Split(new[] { "  Deleting file " }, StringSplitOptions.None))
                    .Select(x => new {
                        activityStart = DateTime.ParseExact(Path.GetFileNameWithoutExtension(file), "yyyy_MM_dd  HH_mm_ss", CultureInfo.InvariantCulture),
                        time = DateTime.ParseExact(x[0].Trim('[', ']'), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                        filename = x[1].Split(new[] { ':' }, 2)[1].Trim().Replace("Y:\\DCIM\\Camera\\", "")
                    })
                    .Select(x => new
                    {
                        main_activity_start_time = x.activityStart,
                        current_time = x.time,
                        operation = "file-deleted",
                        file_name = x.filename,
                        path_to_copy = Path.Combine(targetFolder, x.filename)
                    })
                    .Select(Newtonsoft.Json.JsonConvert.SerializeObject)
                    .ToArray();
                if (deleted.Any())
                {
                    File.AppendAllLines($@"{pathToDevicesFolder}\{device}\FilesActivity.txt", deleted);
                }
            }
        }



        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
    }
}
