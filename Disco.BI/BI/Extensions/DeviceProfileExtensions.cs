using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Models.Repository;
using Disco.Data.Repository;
using Disco.Data.Configuration.Modules;

namespace Disco.BI.Extensions
{
    public static class DeviceProfileExtensions
    {
        public const string ComputerNameExpressionCacheModule = "ComputerNameTemplate";

        public static void ComputerNameInvalidateCache(this DeviceProfile deviceProfile)
        {
            Expressions.ExpressionCache.InvalidateKey(ComputerNameExpressionCacheModule, deviceProfile.Id.ToString());
        }

        public static bool CanDelete(this DeviceProfile dp, DiscoDataContext dbContext)
        {
            // Can't Delete Default Profile (Id: 1)
            if (dp.Id == 1)
                return false;

            // Can't Delete if Contains Devices
            if (dbContext.Devices.Count(d => d.DeviceProfileId == dp.Id) > 0)
                return false;

            return true;
        }
        public static void Delete(this DeviceProfile dp, DiscoDataContext dbContext)
        {
            if (!dp.CanDelete(dbContext))
                throw new InvalidOperationException("The state of this Device Profile doesn't allow it to be deleted");

            // Update Defaults
            if (dbContext.DiscoConfiguration.DeviceProfiles.DefaultDeviceProfileId == dp.Id)
                dbContext.DiscoConfiguration.DeviceProfiles.DefaultDeviceProfileId = 1;
            if (dbContext.DiscoConfiguration.DeviceProfiles.DefaultAddDeviceOfflineDeviceProfileId == dp.Id)
                dbContext.DiscoConfiguration.DeviceProfiles.DefaultAddDeviceOfflineDeviceProfileId = 1;

            // Delete Profile
            dbContext.DeviceProfiles.Remove(dp);
        }

        // Removed 2012-06-14 G# - Properties moved to DeviceProfile model & DB Migrated in DBv3.
        //public static DeviceProfileConfiguration Configuration(this DeviceProfile dp, DiscoDataContext dbContext)
        //{
        //    return dbContext.DiscoConfiguration.DeviceProfiles.DeviceProfile(dp);
        //}

    }
}
