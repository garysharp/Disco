using Disco.Data.Repository;
using Disco.Models.Repository;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Services.Devices.Importing
{
    internal class DeviceImportDatabaseCache : IDeviceImportCache
    {
        private DiscoDataContext Database;

        public DeviceImportDatabaseCache(DiscoDataContext Database)
        {
            this.Database = Database;
        }

        public Device FindDevice(string DeviceSerialNumber)
        {
            return Database.Devices.FirstOrDefault(d => d.SerialNumber == DeviceSerialNumber);
        }

        public IEnumerable<Device> Devices
        {
            get { return Database.Devices; }
        }

        public IEnumerable<DeviceModel> DeviceModels
        {
            get { return Database.DeviceModels; }
        }

        public IEnumerable<DeviceProfile> DeviceProfiles
        {
            get { return Database.DeviceProfiles; }
        }

        public IEnumerable<DeviceBatch> DeviceBatches
        {
            get { return Database.DeviceBatches; }
        }
    }
}