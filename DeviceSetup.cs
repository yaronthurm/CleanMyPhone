using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CleanMyPhone
{
    public class DeviceSetup
    {
        public static async Task SetupDeviceAsync(string appFolder, string deviceID, string ip, int port, string username, string password,
            string sourceFolder, string destinationFolder, int highThreshold, int lowThreshold, int idleTimeInSeconds, bool enableDelete)
        {
            var webDav = new WebDavFileManager(ip, port, username, password);
            if (!(await webDav.ExistAsync("/Cleaner")))
                await webDav.CreateDirectoryAsync("/Cleaner");
            await webDav.CreateOrUpdateFileAsync("/Cleaner/guid.txt", $"id = {deviceID}");

            var tmpSettingsFile = Path.GetTempFileName();
            File.WriteAllLines(tmpSettingsFile, new[] {
                $"port = {port}",
                $"username = {username}",
                $"password = {password}",
                $"source-folder = {sourceFolder}",
                $"destination-folder = {destinationFolder}",
                $"enable-deleting = {enableDelete}",
                $"high-mb-threshold = {highThreshold}",
                $"low-mb-threshold = {lowThreshold}",
                $"idle-time-between-runs-in-seconds = {idleTimeInSeconds}",
                });

            var settingsPath = Path.Combine(appFolder, "Devices", deviceID, "Settings.txt");
            if (!Directory.Exists(Path.GetDirectoryName(settingsPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(settingsPath));
            File.Move(tmpSettingsFile, settingsPath);
        }
    }
}
