using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.Services.Devices.Importing
{
    public interface IDeviceImportContext
    {
        string SessionId { get; }
        string Filename { get; }
        List<Tuple<string, DeviceImportFieldTypes>> Header { get; }
        List<Tuple<string, DeviceImportFieldTypes, Func<string[], string>, Type>> ParsedHeaders { get; }
        List<string[]> RawData { get; }

        List<IDeviceImportRecord> Records { get; }
        int AffectedRecords { get; }
    }
}
