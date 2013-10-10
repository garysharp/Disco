using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Models.Repository;
using Disco.Data.Repository;

namespace Disco.BI.DeviceBI
{
    public static class BatchUtilities
    {
        public static DeviceBatch DefaultNewDeviceBatch(DiscoDataContext Database)
        {
            return new DeviceBatch()
            {
                PurchaseDate = DateTime.Today
            };
        }

    }
}
