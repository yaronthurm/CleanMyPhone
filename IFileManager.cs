using System;

namespace CleanMyPhone
{
    public interface IFileManager
    {
        void CopyWithTimestamps(BasicFileInfo sourceFile, string destinationPath);
        void Delete(BasicFileInfo file);
        bool IsAvailable();
        BasicFileInfo[] ListFiles(string sourceFolder);
        bool Exist(string file);
        string GetFileContentAsString(string file);
    }

    public class BasicFileInfo
    {
        public string Name;
        public string FullName;
        public long SizeInBytes;
        public DateTime? CreationTimeUtc;
        public DateTime LastWriteTimeUtc;

        public BasicFileInfo(string name, string fullName, long sizeInBytes, DateTime? creationTimeUtc, DateTime lastWriteTimeUtc)
        {
            Name = name;
            FullName = fullName;
            SizeInBytes = sizeInBytes;
            CreationTimeUtc = creationTimeUtc;
            LastWriteTimeUtc = lastWriteTimeUtc;
        }
    }
}
