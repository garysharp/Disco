using Disco.Data.Repository;
using Disco.Models.BI.Config;
using Disco.Models.Repository;
using Disco.Services.Authorization;
using Disco.Services.Devices.ManagedGroups;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Users;
using System;
using System.Linq;

namespace Disco.BI.Extensions
{
    public static class DeviceProfileExtensions
    {
        public const string ComputerNameExpressionCacheModule = "ComputerNameTemplate";

        public static void ComputerNameInvalidateCache(this DeviceProfile deviceProfile)
        {
            Expressions.ExpressionCache.InvalidateKey(ComputerNameExpressionCacheModule, deviceProfile.Id.ToString());
        }

        public static OrganisationAddress DefaultOrganisationAddressDetails(this DeviceProfile deviceProfile, DiscoDataContext Database)
        {
            if (deviceProfile.DefaultOrganisationAddress.HasValue)
            {
                return Database.DiscoConfiguration.OrganisationAddresses.GetAddress(deviceProfile.DefaultOrganisationAddress.Value);
            }
            else
            {
                return null;
            }
        }

        public static bool CanDelete(this DeviceProfile dp, DiscoDataContext Database)
        {
            if (!UserService.CurrentAuthorization.Has(Claims.Config.DeviceProfile.Delete))
                return false;

            // Can't Delete Default Profile (Id: 1)
            if (dp.Id == 1)
                return false;

            // Can't Delete if Contains Devices
            if (Database.Devices.Count(d => d.DeviceProfileId == dp.Id) > 0)
                return false;

            return true;
        }
        public static void Delete(this DeviceProfile dp, DiscoDataContext Database)
        {
            if (!dp.CanDelete(Database))
                throw new InvalidOperationException("The state of this Device Profile doesn't allow it to be deleted");

            // Update Defaults
            if (Database.DiscoConfiguration.DeviceProfiles.DefaultDeviceProfileId == dp.Id)
                Database.DiscoConfiguration.DeviceProfiles.DefaultDeviceProfileId = 1;
            if (Database.DiscoConfiguration.DeviceProfiles.DefaultAddDeviceOfflineDeviceProfileId == dp.Id)
                Database.DiscoConfiguration.DeviceProfiles.DefaultAddDeviceOfflineDeviceProfileId = 1;

            // Remove Linked Group
            ActiveDirectory.Context.ManagedGroups.Remove(DeviceProfileDevicesManagedGroup.GetKey(dp));
            ActiveDirectory.Context.ManagedGroups.Remove(DeviceProfileAssignedUsersManagedGroup.GetKey(dp));

            // Delete Profile
            Database.DeviceProfiles.Remove(dp);
        }

    }
}
