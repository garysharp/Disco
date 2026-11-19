using Disco.Models.Services.Devices.Importing;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Disco.Services.Devices.Importing.Intune
{
    internal class IntuneDeviceRecord : IDeviceImportRecord
    {
        public int Index => CsvRecord.RowNumber;
        public string DeviceSerialNumber { get; }

        public IntuneCsvDeviceRecord CsvRecord { get; }
        public IEnumerable<IDeviceImportField> Fields { get; }

        public EntityState RecordAction { get; }
        public bool HasError =>
            Fields.Any(f => !f.FieldAction.HasValue);

        public IntuneDeviceRecord(string parsedSerialNumber, IntuneCsvDeviceRecord csvRecord, IEnumerable<IDeviceImportField> fields, EntityState recordAction)
        {
            DeviceSerialNumber = parsedSerialNumber;
            CsvRecord = csvRecord;
            Fields = fields;
            RecordAction = recordAction;
        }
    }
}
