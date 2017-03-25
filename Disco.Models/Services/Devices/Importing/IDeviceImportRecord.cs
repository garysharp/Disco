using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
