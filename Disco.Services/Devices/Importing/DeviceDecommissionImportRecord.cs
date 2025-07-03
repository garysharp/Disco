using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Disco.Services.Devices.Importing
{
    internal class DeviceDecommissionImportRecord : IDeviceImportRecord
    {
        public int Index { get; }
        public string DeviceSerialNumber { get; }
        public IEnumerable<IDeviceImportField> Fields { get; }
        public EntityState RecordAction { get; }
        public bool HasError { get; }

        public DeviceDecommissionImportRecord(Device device, int index, IEnumerable<IDeviceImportField> fields)
        {
            Index = index;
            DeviceSerialNumber = device.SerialNumber;

            if (fields.Any(f => !f.FieldAction.HasValue))
                RecordAction = EntityState.Detached;
            else if (fields.Any(f => f.FieldAction == EntityState.Modified))
                RecordAction = EntityState.Modified;
            else
                RecordAction = EntityState.Unchanged;

            Fields = fields;
        }
    }
}
