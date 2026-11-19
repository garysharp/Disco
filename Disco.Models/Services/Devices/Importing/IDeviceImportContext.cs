using System.Collections.Generic;

namespace Disco.Models.Services.Devices.Importing
{
    public interface IDeviceImportContext
    {
        string SessionId { get; }
        string Filename { get; }
        string DatasetName { get; }

        int AffectedRecords { get; }
        int RecordCount { get; }

        List<IDeviceImportColumn> Columns { get; }
        List<IDeviceImportRecord> Records { get; }
    }
}
