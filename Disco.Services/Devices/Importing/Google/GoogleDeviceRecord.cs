using Disco.Models.Services.Devices.Importing;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Disco.Services.Devices.Importing.Google
{
    internal sealed class GoogleDeviceRecord : IDeviceImportRecord
    {
        public int Index => CsvRecord.RowNumber;
        public string DeviceSerialNumber { get; }

        public GoogleCsvDeviceRecord CsvRecord { get; }
        public IEnumerable<IDeviceImportField> Fields { get; }

        public EntityState RecordAction { get; }
        public bool HasError =>
            Fields.Any(f => !f.FieldAction.HasValue);

        public GoogleDeviceRecord(string parsedSerialNumber, GoogleCsvDeviceRecord csvRecord, IEnumerable<IDeviceImportField> fields, EntityState recordAction)
        {
            DeviceSerialNumber = parsedSerialNumber;
            CsvRecord = csvRecord;
            Fields = fields;
            RecordAction = recordAction;
        }
    }
}
