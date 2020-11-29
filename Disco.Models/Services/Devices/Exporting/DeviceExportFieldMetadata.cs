using System;

namespace Disco.Models.Services.Devices.Exporting
{
    public class DeviceExportFieldMetadata
    {
        public string Name { get; set; }
        public string ColumnName { get; set; }
        public Type ValueType { get; set; }
        public Func<DeviceExportRecord, object> Accessor { get; set; }
        public Func<object, string> CsvEncoder { get; set; }

        public DeviceExportFieldMetadata(string name, Type valueType, Func<DeviceExportRecord, object> accessor, Func<object, string> csvEncoder)
        {
            Name = name;
            ValueType = valueType;
            Accessor = accessor;
            CsvEncoder = csvEncoder;
        }

        public DeviceExportFieldMetadata(string name, string columnName, Type valueType, Func<DeviceExportRecord, object> accessor, Func<object, string> csvEncoder)
            : this(name, valueType, accessor, csvEncoder)
        {
            ColumnName = columnName;
        }
    }
}
