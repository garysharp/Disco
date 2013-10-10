using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Models.Repository;
using System.IO;
using System.Drawing;
using Disco.Data.Repository;
using Disco.Services.Users;
using Disco.Services.Authorization;

namespace Disco.BI.Extensions
{
    public static class DeviceModelExtensions
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

        #region Actions
        public static bool CanDelete(this DeviceModel dm, DiscoDataContext Database)
        {
            if (!UserService.CurrentAuthorization.Has(Claims.Config.DeviceModel.Delete))
                return false;

            // Can't Delete Default Model (Id: 1)
            if (dm.Id == 1)
                return false;

            // Can't Delete if Contains Devices
            if (Database.Devices.Count(d => d.DeviceModelId == dm.Id) > 0)
                return false;

            return true;
        }
        public static void Delete(this DeviceModel dm, DiscoDataContext Database)
        {
            if (!dm.CanDelete(Database))
                throw new InvalidOperationException("The state of this Device Model doesn't allow it to be deleted");

            // Delete Image
            var deviceModelImagePath = dm.ImageFilePath();
            if (File.Exists(deviceModelImagePath))
                File.Delete(deviceModelImagePath);

            // Delete any Device Model Components
            foreach (var deviceModelComponent in Database.DeviceComponents.Where(dc => dc.DeviceModelId == dm.Id).ToList())
            {
                Database.DeviceComponents.Remove(deviceModelComponent);
            }

            // Delete Model
            Database.DeviceModels.Remove(dm);
        }
        // End Added 2012-11-26 G#
        #endregion

    }
}
