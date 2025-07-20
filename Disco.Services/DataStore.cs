using Disco.Data.Configuration;
using Disco.Data.Repository;
using System;
using System.IO;

namespace Disco.Services
{
    public static class DataStore
    {

        public static string CreateLocation(DiscoDataContext Database, string SubLocation, DateTime? SubSubLocationTimestamp = null)
        {
            return CreateLocation(Database.DiscoConfiguration, SubLocation, SubSubLocationTimestamp);
        }

        public static string CreateLocation(SystemConfiguration DiscoConfiguration, string SubLocation, DateTime? SubSubLocationTimestamp = null)
        {
            string SubSubLocation = string.Empty;
            if (SubSubLocationTimestamp.HasValue)
                SubSubLocation = SubSubLocationTimestamp.Value.ToString(@"yyyy\\MM");

            string storeDirectory = Path.Combine(DiscoConfiguration.DataStoreLocation, SubLocation, SubSubLocation);
            if (!Directory.Exists(storeDirectory))
                Directory.CreateDirectory(storeDirectory);

            return storeDirectory;
        }

        public static void DeleteFile(string FilePath)
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }

        public static void DeleteFiles(params string[] FilePaths)
        {
            foreach (string filePath in FilePaths)
            {
                DeleteFile(filePath);
            }
        }

        public static void WriteFile(string FilePath, Stream FileContent)
        {
            using (FileStream outStream = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                FileContent.CopyTo(outStream);
            }
        }

        public static void WriteFile(string FilePath, byte[] FileContent)
        {
            File.WriteAllBytes(FilePath, FileContent);
        }

    }
}
