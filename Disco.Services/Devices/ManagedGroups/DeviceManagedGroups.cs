using Disco.Data.Repository;
using System.Linq;

namespace Disco.Services.Devices.ManagedGroups
{
    public static class DeviceManagedGroups
    {
        public static void Initialize(DiscoDataContext Database)
        {
            // Device Profiles
            Database.DeviceProfiles
                .Where(dp => dp.DevicesLinkedGroup != null || dp.AssignedUsersLinkedGroup != null)
                .ToList()
                .ForEach(dp =>
                {
                    DeviceProfileDevicesManagedGroup.Initialize(dp);
                    DeviceProfileAssignedUsersManagedGroup.Initialize(dp);
                });

            // Device Batches
            Database.DeviceBatches
                .Where(db => db.DevicesLinkedGroup != null || db.AssignedUsersLinkedGroup != null)
                .ToList()
                .ForEach(db =>
                {
                    DeviceBatchDevicesManagedGroup.Initialize(db);
                    DeviceBatchAssignedUsersManagedGroup.Initialize(db);
                });
        }
    }
}
