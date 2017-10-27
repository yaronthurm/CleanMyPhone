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

        public static void ReadFromConsoleAndSetupDevice(string appFolder){
            string deviceID = ReadFromConsole(
                "Enter desired decive ID (Valid charecters: a-z,A-Z,0-9,'_','-',' ', up to 100 charecters)",
                "Invalid devide ID",
                IsDeviceIDValid,
                x => x);

            string ip = ReadFromConsole("Enter device IP", "Invalid IP format", IsValidIP, x => IPAddress.Parse(x).ToString());
            int port = ReadFromConsole("Enter port", "Invalid port (1-65535)", x => IsValidInt(x, 1, 65535), int.Parse);
            string username = ReadFromConsole("Enter username", "", x => true, x => x);
            string password = ReadFromConsole("Enter password", "", x => true, x => x);
            string sourceFolder = ReadFromConsole("Enter source folder (usually DCIM/Camera)", "", x => true, x => x);
            string destinationFolder = ReadFromConsole("Enter destination folder", "", x => true, x => x);
            bool enableDelete = ReadFromConsole("Enable delete? (Y/N)", "Invalid value", x => IsValidYesNo(x), x => x == "Y" || x == "y");
            int highThreshold = enableDelete ? ReadFromConsole("Enter High threshod [MB] (10,000-50,000)", "Invalid value", x => IsValidInt(x, 10000, 50000), int.Parse) : int.MaxValue;
            int lowThreshold = enableDelete ? ReadFromConsole("Enter Low threshold [MB] (10,000-50,000)", "Invalid value", x => IsValidInt(x, 10000, 50000), int.Parse) : int.MaxValue;
            int idleTimeInSeconds = ReadFromConsole("Enter Idle time in seconds (180-1800)", "Invalid value", x => IsValidInt(x, 180, 1800), int.Parse);

            SetupDevice(appFolder, deviceID, ip, port, username, password, sourceFolder, destinationFolder, highThreshold, lowThreshold, idleTimeInSeconds, enableDelete);
        }


        private static void SetupDevice(string appFolder, string deviceID, string ip, int port, string username, string password,
            string sourceFolder, string destinationFolder, int highThreshold, int lowThreshold, int idleTimeInSeconds, bool enableDelete)
        {
            var webDav = new WebDavFileManager(ip, port, username, password);
            if (!webDav.Exist("/Cleaner"))
                webDav.CreateDirectory("/CleaneR");
            webDav.CreateOrUpdateFile("/Cleaner/guid.txt", $"id = {deviceID}");

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
                $"low-mb-threshold = {lowThreshold}",
                $"idle-time-between-runs-in-seconds = {idleTimeInSeconds}",
                });

            var settingsPath = Path.Combine(appFolder, "Devices", deviceID, "Settings.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(settingsPath));
            File.Move(tmpSettingsFile, settingsPath);
        }


        private static bool IsValidYesNo(string x)
        {
            return x == "Y" || x == "y" || x == "N" || x == "n";
        }

        private static T ReadFromConsole<T>(string text, string errorText, Func<string, bool> checkInput, Func<string, T> selector)
        {
            while (true)
            {
                Console.WriteLine(text);
                var input = Console.ReadLine();
                if (!checkInput(input))
                {
                    Console.WriteLine(errorText);
                }
                else {
                    var ret = selector(input);
                    return ret;
                }
            }
        }

        private static bool IsValidIP(string ip)
        {
            IPAddress tmp;
            var ret = IPAddress.TryParse(ip, out tmp);
            return ret;
        }

        private static bool IsValidInt(string valueString, int min, int max)
        {
            int value;
            if (!int.TryParse(valueString, out value))
                return false;
            if (value < min || value > max)
                return false;
            return true;

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
    }
}
