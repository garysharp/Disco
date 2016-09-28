using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Authorization;
using Disco.Services.Devices.ManagedGroups;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Users;
using System;
using System.Linq;

namespace Disco.Services
{
    public static class DeviceBatchExtensions
    {
        public static bool CanDelete(this DeviceBatch db, DiscoDataContext Database)
        {
            if (!UserService.CurrentAuthorization.Has(Claims.Config.DeviceBatch.Delete))
                return false;

            // Can't Delete if Contains Devices
            var deviceCount = Database.Devices.Count(d => d.DeviceBatchId == db.Id);
            if (deviceCount > 0)
                return false;

            return true;
        }

        public static void Delete(this DeviceBatch db, DiscoDataContext Database)
        {
            if (!db.CanDelete(Database))
                throw new InvalidOperationException("The state of this Device Batch doesn't allow it to be deleted");

            // Remove Linked Group
            ActiveDirectory.Context.ManagedGroups.Remove(DeviceBatchDevicesManagedGroup.GetKey(db));
            ActiveDirectory.Context.ManagedGroups.Remove(DeviceBatchAssignedUsersManagedGroup.GetKey(db));

            // Delete Batch
            Database.DeviceBatches.Remove(db);
        }
    }
}
