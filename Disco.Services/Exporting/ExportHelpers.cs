using ClosedXML.Excel;
using Disco.Models.Exporting;
using Disco.Models.Services.Exporting;
using Disco.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Disco.Services
{
    internal class ExportHelpers
    {
        public static ExportResult WriteExport<T>(IExportOptions options, IScheduledTaskStatus status, List<ExportFieldMetadata<T>> metadata, List<T> records) where T : IExportRecord
        {
            if (records.Count == 0)
                return new ExportResult();

            var filenameWithoutExtension = $"{options.FilenamePrefix}-{status.StartedTimestamp.Value:yyyyMMdd-HHmmss}";
            MemoryStream stream;
            string filename;
            string mimeType;

            switch (options.Format)
            {
                case ExportFormat.Csv:
                    stream = WriteCSV(filenameWithoutExtension, metadata, records, out filename, out mimeType);
                    break;
                case ExportFormat.Xlsx:
                    stream = WriteXlsx(filenameWithoutExtension, options.ExcelWorksheetName, options.ExcelTableName, metadata, records, out filename, out mimeType);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported export format: {options.Format}");
            }

            return new ExportResult()
            {
                Result = stream,
                RecordCount = records.Count,
                Filename = filename,
                MimeType = mimeType,
            };
        }

        private static MemoryStream WriteCSV<T>(string filenameWithoutExtension, List<ExportFieldMetadata<T>> metadata, List<T> records, out string filename, out string mimeType) where T : IExportRecord
        {
            var stream = new MemoryStream();
            mimeType = "text/csv";
            filename = $"{filenameWithoutExtension}.csv";

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

        private static MemoryStream WriteXlsx<T>(string filenameWithoutExtension, string worksheetName, string tableName, List<ExportFieldMetadata<T>> metadata, List<T> records, out string filename, out string mimeType) where T : IExportRecord
        {
            var stream = new MemoryStream();
            mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            filename = $"{filenameWithoutExtension}.xlsx";

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
    }
}
