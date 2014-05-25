using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.Services.Devices.Importing
{
    public interface IDeviceImportField
    {
        DeviceImportFieldTypes FieldType { get; }
        EntityState? FieldAction { get; }
        
        string ErrorMessage { get; }

        object RawParsedValue { get; }
        string FriendlyValue { get; }
        string FriendlyPreviousValue { get; }
    }
}
