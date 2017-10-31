using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CleanMyPhone
{
    public class CleanerSettings
    {
        public int Port { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string SourceFolder { get; private set; }
        public string DestinationFolder { get; private set; }
        public int HighMbThreshold { get; private set; }
        public int LowMbThreshold { get; private set; }
        public int IdleTimeBetweenRunsInSeconds { get; private set; }
        public bool EnableDeleting { get; private set; }

        private string SettingsFile { get; set; }
        private string DeviceFolder { get; set; }

        public string GetSettingsFile() => this.SettingsFile;
        public string GetDeviceFolder() => this.DeviceFolder;

        public static CleanerSettings LoadFromFile(string filename)
        {
            var configValues = File.ReadAllLines(filename)
                .Select(x => x.Split(new char[] { '=' }, 2))
                .Select(x => new { key = x[0].Trim(), value = x[1].Trim() })
                .ToDictionary(x => x.key, x => x.value);

            var ret = new CleanerSettings();
            ret.SettingsFile = filename;
            ret.DeviceFolder = Path.GetDirectoryName(filename);
            ret.Port = int.Parse(configValues["port"]);
            ret.Username = configValues["username"];
            ret.Password = configValues["password"];
            ret.SourceFolder = configValues["source-folder"];
            ret.DestinationFolder = configValues["destination-folder"];
            ret.HighMbThreshold = int.Parse(configValues["high-mb-threshold"]);
            ret.LowMbThreshold = int.Parse(configValues["low-mb-threshold"]);
            ret.IdleTimeBetweenRunsInSeconds = int.Parse(configValues["idle-time-between-runs-in-seconds"]);
            ret.EnableDeleting = bool.Parse(configValues["enable-deleting"]);

            return ret;
        }

        public void Save()
        {
            File.WriteAllLines(this.SettingsFile, new[] {
                $"port = {this.Port}",
                $"username = {this.Username}",
                $"password = {this.Password}",
                $"source-folder = {this.SourceFolder}",
                $"destination-folder = {this.DestinationFolder}",
                $"enable-deleting = {this.EnableDeleting}",
                $"high-mb-threshold = {this.HighMbThreshold}",
                $"low-mb-threshold = {this.LowMbThreshold}",
                $"idle-time-between-runs-in-seconds = {this.IdleTimeBetweenRunsInSeconds}",
                });
        }

        public static Dictionary<string, CleanerSettings> GetAllConfigs(string appFolder)
        {
            var devicesDir = Path.Combine(appFolder, "Devices");
            var ret = Directory.GetDirectories(devicesDir)
                .Select(x => new
                {
                    id = Path.GetFileName(x),
                    settings = CleanerSettings.LoadFromFile(Path.Combine(x, "Settings.txt"))
                })
                .ToDictionary(x => x.id, x => x.settings, StringComparer.OrdinalIgnoreCase);
            return ret;
        }
    }
}