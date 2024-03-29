﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CleanMyPhone
{
    public interface ICleanerSettings
    {
        bool Enabled { get; }
        string Username { get; }
        string Password { get; }
        int Port { get; }
        int IdleTimeBetweenRunsInSeconds { get; }
        PerFolderSettings[] FoldersSettings { get;  }

        string GetSettingsFile();
        string GetDeviceFolder();
        void Save();
    }

    public class PerFolderSettings
    {
        public bool EnableDeleting;
        public int HighMbThreshold;
        public int LowMbThreshold;
        public string SourceFolder;
        public string DestinationFolder;        
    }

    public static class CleanerSettingsFactory {

        public static Dictionary<string, ICleanerSettings> GetAllConfigs(string appFolder)
        {
            var devicesDir = Path.Combine(appFolder, "Devices");
            if (!Directory.Exists(devicesDir))
                Directory.CreateDirectory(devicesDir);
            var ret = Directory.GetDirectories(devicesDir)
                .Select(x =>
                {
                    var path_v1 = Path.Combine(x, "Settings.txt");
                    var path_v2 = Path.Combine(x, "Settings_v2.txt");
                    if (File.Exists(path_v2))
                        return new
                        {
                            id = Path.GetFileName(x),
                            settings = (ICleanerSettings)CleanerSettingsV2.LoadFromFile(path_v2)
                        };
                    else
                        return new
                        {
                            id = Path.GetFileName(x),
                            settings = (ICleanerSettings)CleanerSettingsV1.LoadFromFile(path_v1)
                        };
                })
                .ToDictionary(x => x.id, x => x.settings, StringComparer.OrdinalIgnoreCase);
            return ret;
        }
    }

    public class CleanerSettingsV1 : ICleanerSettings
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

        public PerFolderSettings[] FoldersSettings => new[] {
            new PerFolderSettings {
                EnableDeleting = this.EnableDeleting,
                HighMbThreshold = this.HighMbThreshold,
                LowMbThreshold = this.LowMbThreshold,
                SourceFolder = this.SourceFolder,
                DestinationFolder = this.DestinationFolder
            }
        };

        public string GetSettingsFile() => this.SettingsFile;
        public string GetDeviceFolder() => this.DeviceFolder;


        private CleanerSettingsV1() { }


        public static CleanerSettingsV1 LoadFromFile(string filename)
        {
            var configValues = File.ReadAllLines(filename)
                .Select(x => x.Split(new char[] { '=' }, 2))
                .Select(x => new { key = x[0].Trim(), value = x[1].Trim() })
                .ToDictionary(x => x.key, x => x.value);

            var ret = new CleanerSettingsV1();
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
    }

    public class CleanerSettingsV2 : ICleanerSettings
    {
        public bool Enabled { get; private set; }
        public int Port { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public int IdleTimeBetweenRunsInSeconds { get; private set; }


        private string SettingsFile { get; set; }
        private string DeviceFolder { get; set; }

        public PerFolderSettings[] FoldersSettings { get; private set; }

        public string GetSettingsFile() => this.SettingsFile;
        public string GetDeviceFolder() => this.DeviceFolder;


        private CleanerSettingsV2() { }


        public static CleanerSettingsV2 LoadFromFile(string filename)
        {
            var ret = new CleanerSettingsV2();
            ret.SettingsFile = filename;
            ret.DeviceFolder = Path.GetDirectoryName(filename);
            ret.UpdateFromText(File.ReadAllText(filename));
            return ret;
        }

        public string ToText()
        {
            var json = new JObject(
                new JProperty("enabled", this.Enabled),
                new JProperty("port", this.Port),
                new JProperty("username", this.Username),
                new JProperty("password", this.Password),
                new JProperty("idle-time-between-runs-in-seconds", this.IdleTimeBetweenRunsInSeconds),
                new JProperty("folders", new JArray(
                    this.FoldersSettings.Select(x => 
                        new JObject(
                            new JProperty("source-folder", x.SourceFolder),
                            new JProperty("destination-folder", x.DestinationFolder),
                            new JProperty("enable-deleting", x.EnableDeleting),
                            new JProperty("high-mb-threshold", x.HighMbThreshold),
                            new JProperty("low-mb-threshold", x.LowMbThreshold)
                        )))));

            return json.ToString(Newtonsoft.Json.Formatting.Indented);
        }

        public void UpdateFromText(string text) {
            var json = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(text);
            this.Enabled = json.Value<bool>("enabled");
            this.Port = json.Value<int>("port");
            this.Username = json.Value<string>("username");
            this.Password = json.Value<string>("password");
            this.IdleTimeBetweenRunsInSeconds = json.Value<int>("idle-time-between-runs-in-seconds");
            this.FoldersSettings = json.Value<JArray>("folders")
                .Select(x => new PerFolderSettings
                {
                    SourceFolder = x.Value<string>("source-folder"),
                    DestinationFolder = x.Value<string>("destination-folder"),
                    EnableDeleting = x.Value<bool>("enable-deleting"),
                    HighMbThreshold = x.Value<int>("high-mb-threshold"),
                    LowMbThreshold = x.Value<int>("low-mb-threshold"),
                })
                .ToArray();
        }
        
        public void Save()
        {
            File.WriteAllText(this.SettingsFile, this.ToText());
        }        
    }
}