using System;

namespace Disco.Models.Exporting
{
    public class ExportFieldMetadata<T> where T : IExportRecord
    {
        public string Name { get; set; }
        public string ColumnName { get; set; }
        public Type ValueType { get; set; }
        public Func<T, object> Accessor { get; set; }
        public Func<object, string> CsvEncoder { get; set; }

        public ExportFieldMetadata(string name, Type valueType, Func<T, object> accessor, Func<object, string> csvEncoder)
        {
            Name = name;
            ValueType = valueType;
            Accessor = accessor;
            CsvEncoder = csvEncoder;
        }

        public ExportFieldMetadata(string name, string columnName, Type valueType, Func<T, object> accessor, Func<object, string> csvEncoder)
            : this(name, valueType, accessor, csvEncoder)
        {
            ColumnName = columnName;
        }
    }
}
