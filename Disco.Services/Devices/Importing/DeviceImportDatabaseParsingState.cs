using Disco.Data.Repository;
using Disco.Models.Repository;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Disco.Services.Devices.Importing
{
    internal class DeviceImportDatabaseParsingState
    {
        public DiscoDataContext Database { get; }
        public List<DeviceModel> DeviceModels { get; }
        public List<DeviceProfile> DeviceProfiles { get; }
        public List<DeviceBatch> DeviceBatches { get; }

        public DeviceImportDatabaseParsingState(DiscoDataContext database)
        {
            Database = database;
            DeviceModels = database.DeviceModels.ToList();
            DeviceProfiles = database.DeviceProfiles.ToList();
            DeviceBatches = database.DeviceBatches.ToList();
        }

        public Device FindDevice(string serialNumber)
        {
            return Database.Devices
                .Include(d => d.DeviceDetails)
                .Include(d => d.AssignedUser)
                .FirstOrDefault(d => d.SerialNumber == serialNumber);
        }
    }
}
