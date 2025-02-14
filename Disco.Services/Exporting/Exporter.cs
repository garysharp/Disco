using ClosedXML.Excel;
using Disco.Data.Repository;
using Disco.Models.Exporting;
using Disco.Models.Services.Exporting;
using Disco.Services.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Disco.Services.Exporting
{
    public static class Exporter
    {
        public static ExportResult Export<T, R>(IExport<T, R> export, DiscoDataContext database, IScheduledTaskStatus status)
            where T : IExportOptions, new()
            where R : IExportRecord
        {
            MemoryStream stream;
            string mimeType;

            status.UpdateStatus(1, $"Exporting {export.Name}", "Gathering data");

            var records = export.BuildRecords(database, status);

            status.UpdateStatus(70, "Building metadata");

            var metadata = export.BuildMetadata(database, records, status);

            if (metadata.Count == 0)
                throw new ArgumentException("At least one export field must be specified", nameof(export.Options));

            var filenameBuilder = new StringBuilder();
            filenameBuilder.Append(export.FilenamePrefix);
            filenameBuilder.Append('-');
            filenameBuilder.Append(status.StartedTimestamp.Value.ToString("yyyyMMdd-HHmmss"));

            status.UpdateStatus(80, $"Rendering {records.Count} records for export");

            switch (export.Options.Format)
            {
                case ExportFormat.Csv:
                    filenameBuilder.Append(".csv");
                    mimeType = "text/csv";
                    stream = WriteCSV(metadata, records);
                    break;
                case ExportFormat.Xlsx:
                    filenameBuilder.Append(".xlsx");
                    mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    stream = WriteXlsx(export.ExcelWorksheetName, export.ExcelTableName, metadata, records);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported export format: {export.Options.Format}");
            }

            return new ExportResult()
            {
                Result = stream,
                RecordCount = records.Count,
                Filename = filenameBuilder.ToString(),
                MimeType = mimeType,
            };
        }

        private static MemoryStream WriteCSV<T>(List<ExportMetadataField<T>> metadata, List<T> records) where T : IExportRecord
        {
            var stream = new MemoryStream();

            using (StreamWriter writer = new StreamWriter(stream, Encoding.Default, 0x400, true))
            {
                // Header
                writer.Write('"');
                writer.Write(string.Join("\",\"", metadata.Select(m => m.ColumnName)));
                writer.Write('"');

                // Records
                foreach (var record in records)
                {
                    writer.WriteLine();
                    for (int i = 0; i < metadata.Count; i++)
                    {
                        if (i != 0)
                        {
                            writer.Write(',');
                        }
                        var value = metadata[i].Accessor(record);
                        writer.Write(metadata[i].CsvEncoder(value));
                    }
                }
            }

            stream.Position = 0;
            return stream;
        }
        private static MemoryStream WriteXlsx<T>(string worksheetName, string tableName, List<ExportMetadataField<T>> metadata, List<T> records) where T : IExportRecord
        {
            var stream = new MemoryStream();

            // Create DataTable
            var dataTable = new DataTable();
            foreach (var field in metadata)
            {
                dataTable.Columns.Add(field.ColumnName, field.ValueType);
            }
            foreach (var record in records)
            {
                dataTable.Rows.Add(metadata.Select(m => m.Accessor(record)).ToArray());
            }

            using (var xlWorkbook = new XLWorkbook())
            {
                var sheet = xlWorkbook.AddWorksheet(worksheetName);
                var table = sheet.Cell(1, 1).InsertTable(dataTable, tableName);
                table.Theme = XLTableTheme.TableStyleMedium2;

                table.Columns().ForEach(c => c.WorksheetColumn().AdjustToContents(2, 15, 30));

                xlWorkbook.SaveAs(stream);
            }

            stream.Position = 0;
            return stream;
        }

        public static void Add<O, R, V>(this ExportMetadata<O, R> metadata, Expression<Func<O, bool>> optionAccessor, Func<R, V> valueAccessor, Func<object, string> csvValueEncoder = null, string columnName = null)
            where O : IExportOptions
            where R : IExportRecord
        {
            // is field enabled?
            if (!optionAccessor.Compile().Invoke(metadata.Options))
                return;

            if (columnName is null)
            {
                var member = ((MemberExpression)optionAccessor.Body).Member;
                var attribute = (DisplayAttribute)member.GetCustomAttributes(typeof(DisplayAttribute), false).Single();

                if (metadata.IgnoreShortNames.Contains(attribute.ShortName))
                    columnName = attribute.Name;
                else
                    columnName = $"{attribute.ShortName} {attribute.Name}";
            }

            metadata.Add(columnName, valueAccessor, csvValueEncoder);
        }
        public static void Add<O, R, V>(this ExportMetadata<O, R> metadata, string columnName, Func<R, V> valueAccessor, Func<object, string> csvValueEncoder = null)
            where O : IExportOptions
            where R : IExportRecord
        {
            var valueType = typeof(V);
            if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Nullable<>))
                valueType = valueType.GetGenericArguments()[0];

            if (csvValueEncoder is null)
                csvValueEncoder = CsvEncoders.GetEncoder<V>();

            var field = new ExportMetadataField<R>(columnName, valueType, (R i) => valueAccessor(i), csvValueEncoder);
            metadata.Add(field);
        }

        public static class CsvEncoders
        {
            private static Dictionary<Type, Func<object, string>> encoders = new Dictionary<Type, Func<object, string>>()
            {
                { typeof(string), StringEncoder },
                { typeof(object), ObjectToStringEncoder },
                { typeof(byte), ToStringEncoder },
                { typeof(byte?), ToStringEncoder },
                { typeof(decimal), ToStringEncoder },
                { typeof(decimal?), ToStringEncoder },
                { typeof(double), ToStringEncoder },
                { typeof(double?), ToStringEncoder },
                { typeof(float), ToStringEncoder },
                { typeof(float?), ToStringEncoder },
                { typeof(int), ToStringEncoder },
                { typeof(int?), ToStringEncoder },
                { typeof(uint), ToStringEncoder },
                { typeof(uint?), ToStringEncoder },
                { typeof(long), ToStringEncoder },
                { typeof(long?), ToStringEncoder },
                { typeof(ulong), ToStringEncoder },
                { typeof(ulong?), ToStringEncoder },
                { typeof(short), ToStringEncoder },
                { typeof(short?), ToStringEncoder },
                { typeof(ushort), ToStringEncoder },
                { typeof(ushort?), ToStringEncoder },
                { typeof(bool), ToStringEncoder },
                { typeof(bool?), ToStringEncoder },
                { typeof(DateTime), DateTimeEncoder },
                { typeof(DateTime?), NullableDateTimeEncoder },
            };

            public static readonly string DateFormat = "yyyy-MM-dd";
            public static readonly string DateTimeFormat = DateFormat + " HH:mm:ss";

            public static string StringEncoder(object o) => o == null ? null : $"\"{((string)o).Replace("\"", "\"\"")}\"";
            public static string ObjectToStringEncoder(object o) => o == null ? null : o is string s ? StringEncoder(s) : o.ToString();
            public static string ToStringEncoder(object o) => o == null ? null : o.ToString();
            public static string CurrencyEncoder(object o) => ((decimal)o).ToString("C");
            public static string NullableCurrencyEncoder(object o) => ((decimal?)o).HasValue ? ((decimal?)o).Value.ToString("C") : null;
            public static string DateEncoder(object o) => ((DateTime)o).ToString(DateFormat);
            public static string NullableDateEncoder(object o) => ((DateTime?)o).HasValue ? DateEncoder(o) : null;
            public static string DateTimeEncoder(object o) => ((DateTime)o).ToString(DateTimeFormat);
            public static string NullableDateTimeEncoder(object o) => ((DateTime?)o).HasValue ? DateTimeEncoder(o) : null;

            public static Func<object, string> GetEncoder<T>()
                => GetEncoder(typeof(T));

            public static Func<object, string> GetEncoder(Type type)
            {
                if (encoders.TryGetValue(type, out var encoder))
                    return encoder;
                else
                    throw new NotSupportedException($"No encoder for type {type}");
            }
        }

    }
}
