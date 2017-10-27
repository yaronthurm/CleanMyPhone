﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanMyPhone
{
    public class DeviceSetup
    {
        public static void SetupDevice(string appFolder, string deviceID, string ip, string port, string username, string password,
            string sourceFolder, string destinationFolder, string highThreshold, string lowThreshold, string idleTimeInSeconds)
        {
            var tmpSettingsFile = Path.GetTempFileName();
            File.WriteAllLines(tmpSettingsFile, new[] {
                $"port = {port}",
                $"username = {username}",
                $"password = {password}",
                $"source-folder = {sourceFolder}",
                $"destination-folder = {destinationFolder}",
                $"high-mb-threshold = {highThreshold}",
                $"low-mb-threshold = {lowThreshold}",
                $"low-mb-threshold = {lowThreshold}",
                $"idle-time-between-runs-in-seconds = {idleTimeInSeconds}"
                });

            var settingsPath = Path.Combine(appFolder, "Devices", deviceID, "Settings.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(settingsPath));
            File.Move(tmpSettingsFile, settingsPath);


            var webDav = new WebDavFileManager(ip, int.Parse(port), username, password);
            webDav.CreateDirectory("/Cleaner");
            webDav.CreateFile("/Cleaner/guid.txt", $"id = {deviceID}");
        }
    }
}
