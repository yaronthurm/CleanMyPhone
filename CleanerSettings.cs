using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CleanMyPhone
{
    public class CleanerSettings
    {
        public bool Enabled { get; private set; }
        public int Port { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string SourceFolder { get; private set; }
        public string DestinationFolder { get; private set; }
        public bool EnableDeleting { get; private set; }
        public int HighMbThreshold { get; private set; }
        public int LowMbThreshold { get; private set; }
        public int IdleTimeBetweenRunsInSeconds { get; private set; }
        

        private string SettingsFile { get; set; }
        private string DeviceFolder { get; set; }

        public string GetSettingsFile() => this.SettingsFile;
        public string GetDeviceFolder() => this.DeviceFolder;


        private CleanerSettings() { }


        public static CleanerSettings LoadFromFile(string filename)
        {
            var configValues = File.ReadAllLines(filename)
                .Select(x => x.Split(new char[] { '=' }, 2))
                .Select(x => new { key = x[0].Trim(), value = x[1].Trim() })
                .ToDictionary(x => x.key, x => x.value);

            var ret = new CleanerSettings();
            ret.SettingsFile = filename;
            ret.Enabled = GetValueWithDefault(configValues, "enabled", bool.Parse, false);
            ret.DeviceFolder = Path.GetDirectoryName(filename);
            ret.Port = GetValueWithDefault(configValues, "port", int.Parse, 8080);
            ret.Username = GetValueWithDefault(configValues, "username", x => x, "");
            ret.Password = GetValueWithDefault(configValues, "password", x => x, "");
            ret.SourceFolder = GetValueWithDefault(configValues, "source-folder", x => x, "/DCIM/Camera");
            ret.DestinationFolder = GetValueWithDefault(configValues, "destination-folder", x => x, "");
            ret.EnableDeleting = GetValueWithDefault(configValues, "enable-deleting", bool.Parse, false);
            ret.HighMbThreshold = GetValueWithDefault(configValues, "high-mb-threshold", int.Parse, 0);
            ret.LowMbThreshold = GetValueWithDefault(configValues, "low-mb-threshold", int.Parse, 0);
            ret.IdleTimeBetweenRunsInSeconds = GetValueWithDefault(configValues, "idle-time-between-runs-in-seconds", int.Parse, 180);
            
            
            return ret;
        }

        private static T GetValueWithDefault<T>(Dictionary<string, string> data, string keyname, Func<string, T> selector, T defaultValue)
        {
            string value;
            if (data.TryGetValue(keyname, out value))
            {
                return selector(value);
            }
            else
            {
                return defaultValue;
            }
        }


        public void Save()
        {
            File.WriteAllLines(this.SettingsFile, new[] {
                $"enabled = {this.Enabled}",
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
            if (!Directory.Exists(devicesDir))
                Directory.CreateDirectory(devicesDir);
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