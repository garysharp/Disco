using Disco.Data.Repository;
using Disco.Models.Repository;
using System;

namespace Disco.Services.Devices
{
    public static class DeviceBatches
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
