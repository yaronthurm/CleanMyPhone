using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CleanMyPhone
{
    public class WebDavFileManager : IFileManager
    {
        private WebDavClient _myWebDavClient;

        public WebDavFileManager(string server, int port, string username, string password)
        {
            NetworkCredential credentials = new NetworkCredential(username, password);
            _myWebDavClient = new WebDavClient(credentials);
            _myWebDavClient.Server = server;
            _myWebDavClient.Port = port;
        }
        

        public void CopyWithTimestamps(BasicFileInfo sourceFile, string destinationPath)
        {
            var newFile = File.Create(destinationPath);
            var fileToCopy = _myWebDavClient.GetContentAsStream(sourceFile.FullName).Result;
            fileToCopy.CopyTo(newFile);
            newFile.Close();

            File.SetCreationTimeUtc(destinationPath, sourceFile.CreationTimeUtc ?? sourceFile.LastWriteTimeUtc);
            File.SetLastAccessTimeUtc(destinationPath, sourceFile.LastWriteTimeUtc);
            File.SetLastWriteTimeUtc(destinationPath, sourceFile.LastWriteTimeUtc);
        }

        public void Delete(BasicFileInfo file)
        {
            _myWebDavClient.Delete(file.FullName).Wait();
        }

        public string GetFileContentAsString(string filename)
        {
            var ret = _myWebDavClient.GetContentAsString(filename).Result;
            return ret;
        }

        public bool Exist(string filename)
        {
            return ExistAsync(filename).Result;
        }

        public async Task<bool> ExistAsync(string filename)
        {
            try
            {
                var file = await _myWebDavClient.TryGetItem(filename);
                return file.Found;
            }
            catch
            {
                return false;
            }
        }

        public bool IsAvailable()
        {
            var root = _myWebDavClient.TryGetItem("/").Result;
            return root.Found;
        }

        public BasicFileInfo[] ListFiles(string _sourceFolder)
        {
            var ret = _myWebDavClient.ListItems(_sourceFolder).Result
                .Where(x => !x.IsCollection)
                .Select(x => new BasicFileInfo(
                    x.DisplayName,
                    x.Href,
                    x.ContentLength.Value,
                    ParseToDateTime(x.CreationDate),
                    ParseToDateTime(x.LastModified).Value))
                    .ToArray();
            return ret;
        }

        public void CreateDirectory(string path)
        {
            _myWebDavClient.CreateDir(path).Wait();
        }

        public Task CreateDirectoryAsync(string path)
        {
            return _myWebDavClient.CreateDir(path);
        }

        public void CreateOrUpdateFile(string filename, string fileContent)
        {
            _myWebDavClient.CreateOrUpdateFile(filename, fileContent).Wait();
        }

        public Task CreateOrUpdateFileAsync(string filename, string fileContent)
        {
            return _myWebDavClient.CreateOrUpdateFile(filename, fileContent);
        }

        private static DateTime? ParseToDateTime(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            DateTime ret;
            if (DateTime.TryParseExact(value, "ddd, d MMM yyyy HH:mm:ss GMT+00:00", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out ret))
                return ret;
            return null;
        }
    }
}
