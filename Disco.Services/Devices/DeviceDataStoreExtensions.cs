using Disco.Models.Repository;
using System;
using System.Drawing;
using System.IO;

namespace Disco.Services
{
    public static class DeviceDataStoreExtensions
    {
        public static bool ImageImport(this DeviceModel deviceModel, Stream ImageStream)
        {
            try
            {
                using (Bitmap inputBitmap = new Bitmap(ImageStream))
                {
                    using (Image outputBitmap = inputBitmap.ResizeImage(256, 256))
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            outputBitmap.SavePng(ms);
                            ms.Position = 0;

                            var deviceModelImagePath = deviceModel.ImageFilePath();


                            using (var storeStream = new FileStream(deviceModelImagePath, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                ms.CopyTo(storeStream);
                            }
                            //deviceModel.Image = ms.ToArray();
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static FileStream Image(this DeviceModel deviceModel)
        {
            var deviceModelImagePath = deviceModel.ImageFilePath();

            if (File.Exists(deviceModelImagePath))
                return new FileStream(deviceModelImagePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            else
                return null;
        }

        public static string ImageFilePath(this DeviceModel deviceModel)
        {
            var configCache = new Disco.Data.Configuration.SystemConfiguration(null);

            var deviceModelImagesDataStore = DataStore.CreateLocation(configCache, "DeviceModelImages");

            return Path.Combine(deviceModelImagesDataStore, string.Format("{0}.png", deviceModel.Id));
        }

        public static string ImageHash(this DeviceModel deviceModel)
        {
            var deviceModelImagePath = deviceModel.ImageFilePath();

            if (File.Exists(deviceModelImagePath))
                return File.GetLastWriteTimeUtc(deviceModelImagePath).ToBinary().ToString();
            else
                return "-1";
        }
    }
}
