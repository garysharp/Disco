using System;
using System.Collections.Generic;

namespace Disco.Models.Services.Devices.Importing
{
    public interface IDeviceImportContext
    {
        string SessionId { get; }
        string Filename { get; }
        string DatasetName { get; }
        int ColumnCount { get; }
        IEnumerable<IDeviceImportColumn> Columns { get; }
        IDeviceImportColumn GetColumn(int Index);
        void SetColumnType(int Index, DeviceImportFieldTypes Type);
        int? GetColumnByType(DeviceImportFieldTypes FieldType);

        int RecordCount { get; }

        IDeviceImportDataReader GetDataReader();
        IEnumerable<KeyValuePair<DeviceImportFieldTypes, Type>> GetFieldHandlers();

        List<IDeviceImportRecord> Records { get; set; }
        int AffectedRecords { get; set; }
        bool AllowBacktracking { get; }
    }
}
