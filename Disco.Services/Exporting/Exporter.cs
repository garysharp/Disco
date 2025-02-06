using ClosedXML.Excel;
using Disco.Data.Repository;
using Disco.Models.Exporting;
using Disco.Models.Services.Exporting;
using Disco.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Disco.Services.Exporting
{
    public static class Exporter
    {
        public static ExportResult Export<T, R>(DiscoDataContext database, IExportContext<T, R> context, IScheduledTaskStatus status)
            where T : IExportOptions, new()
            where R : IExportRecord
        {
            MemoryStream stream;
            string mimeType;

            status.UpdateStatus(1, $"Exporting {context.Name}", "Gathering data");

            var records = context.BuildRecords(database, status);

            status.UpdateStatus(70, "Building metadata");

            var metadata = context.BuildMetadata(database, records, status);

            if (metadata.Count == 0)
                throw new ArgumentException("At least one export field must be specified", nameof(context.Options));

            var filenameBuilder = new StringBuilder();
            filenameBuilder.Append(context.SuggestedFilenamePrefix);
            if (context.TimestampSuffix)
            {
                filenameBuilder.Append('-');
                filenameBuilder.Append(status.StartedTimestamp.Value.ToString("yyyyMMdd-HHmmss"));
            }

            status.UpdateStatus(80, $"Rendering {records.Count} records for export");

            switch (context.Options.Format)
            {
                case ExportFormat.Csv:
                    filenameBuilder.Append(".csv");
                    mimeType = "text/csv";
                    stream = WriteCSV(metadata, records);
                    break;
                case ExportFormat.Xlsx:
                    filenameBuilder.Append(".xlsx");
                    mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    stream = WriteXlsx(context.ExcelWorksheetName, context.ExcelTableName, metadata, records);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported export format: {context.Options.Format}");
            }

            return new ExportResult()
            {
                Result = stream,
                RecordCount = records.Count,
                Filename = filenameBuilder.ToString(),
                MimeType = mimeType,
            };
        }

        private static MemoryStream WriteCSV<T>(List<ExportFieldMetadata<T>> metadata, List<T> records) where T : IExportRecord
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

        private static MemoryStream WriteXlsx<T>(string worksheetName, string tableName, List<ExportFieldMetadata<T>> metadata, List<T> records) where T : IExportRecord
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
    }
}
