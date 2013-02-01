using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Models.Repository;
using Disco.Data.Repository;

namespace Disco.BI.Extensions
{
    public static class DeviceBatchExtensions
    {
        public static bool CanDelete(this DeviceBatch db, DiscoDataContext dbContext)
        {
            // Can't Delete if Contains Devices
            var deviceCount = dbContext.Devices.Count(d => d.DeviceBatchId == db.Id);
            if (deviceCount > 0)
                return false;

            return true;
        }

        public static void Delete(this DeviceBatch db, DiscoDataContext dbContext)
        {
            if (!db.CanDelete(dbContext))
                throw new InvalidOperationException("The state of this Device Batch doesn't allow it to be deleted");

            // Delete Batch
            dbContext.DeviceBatches.Remove(db);
        }
    }
}
