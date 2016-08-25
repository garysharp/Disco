using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services;
using Disco.Services.Authorization;
using Disco.Services.Users;
using System;
using System.IO;
using System.Linq;

namespace Disco.BI.Extensions
{
    public static class DeviceModelExtensions
    {
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
