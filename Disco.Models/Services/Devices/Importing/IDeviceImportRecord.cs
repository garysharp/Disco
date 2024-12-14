using System.Collections.Generic;
using System.Data;

namespace Disco.Models.Services.Devices.Importing
{
    public interface IDeviceImportRecord
    {
        int Index { get; }
        string DeviceSerialNumber { get; }

        IEnumerable<IDeviceImportField> Fields { get; }

        EntityState RecordAction { get; }

        bool HasError { get; }
    }
}
