using Disco.Data.Repository;
using Disco.Models.Repository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Services.Devices.Importing
{
    internal class DeviceImportInMemoryCache : IDeviceImportCache
    {
        private DiscoDataContext Database;

        private Lazy<IEnumerable<Device>> devices;
        private Lazy<IEnumerable<DeviceModel>> deviceModels;
        private Lazy<IEnumerable<DeviceProfile>> deviceProfiles;
        private Lazy<IEnumerable<DeviceBatch>> deviceBatches;

        public DeviceImportInMemoryCache(DiscoDataContext Database)
        {
            this.Database = Database;

            devices = new Lazy<IEnumerable<Device>>(() => Database.Devices.Include("DeviceDetails").ToList());
            deviceModels = new Lazy<IEnumerable<DeviceModel>>(() => Database.DeviceModels.ToList());
            deviceProfiles = new Lazy<IEnumerable<DeviceProfile>>(() => Database.DeviceProfiles.ToList());
            deviceBatches = new Lazy<IEnumerable<DeviceBatch>>(() => Database.DeviceBatches.ToList());
        }

        public Device FindDevice(string DeviceSerialNumber)
        {
            return devices.Value.FirstOrDefault(d => d.SerialNumber.Equals(DeviceSerialNumber, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<Device> Devices
        {
            get { return devices.Value; }
        }

        public IEnumerable<DeviceModel> DeviceModels
        {
            get { return deviceModels.Value; }
        }

        public IEnumerable<DeviceProfile> DeviceProfiles
        {
            get { return deviceProfiles.Value; }
        }

        public IEnumerable<DeviceBatch> DeviceBatches
        {
            get { return deviceBatches.Value; }
        }
    }
}