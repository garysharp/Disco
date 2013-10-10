using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Models.Repository;
using Disco.Data.Repository;
using Disco.Services.Users;
using Disco.Services.Authorization;

namespace Disco.BI.Extensions
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

            // Delete Batch
            Database.DeviceBatches.Remove(db);
        }
    }
}
