using System;

namespace Disco.Models.Exporting
{
    public class ExportMetadataField<T> where T : IExportRecord
    {
        public string ColumnName { get; }
        public Type ValueType { get; }
        public Func<T, object> Accessor { get; }
        public Func<object, string> CsvEncoder { get; }

        public ExportMetadataField(string columnName, Type valueType, Func<T, object> accessor, Func<object, string> csvEncoder)
        {
            ColumnName = columnName;
            ValueType = valueType;
            Accessor = accessor;
            CsvEncoder = csvEncoder;
        }
    }
}
