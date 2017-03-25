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

        public DeviceExportFieldMetadata(string Name, Type ValueType, Func<DeviceExportRecord, object> Accessor, Func<object, string> CsvEncoder)
        {
            this.Name = Name;
            this.ValueType = ValueType;
            this.Accessor = Accessor;
            this.CsvEncoder = CsvEncoder;
        }
    }
}
