using ClosedXML.Excel;
using Disco.Models.Services.Devices.Importing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Disco.Services.Devices.Importing
{
    public class XlsxDeviceImportContext : BaseDeviceImportContext
    {
        private bool hasHeaderRow;
        private List<object[]> rawData;

        public XlsxDeviceImportContext(string Filename, bool HasHeaderRow, Stream XlsxStream)
            : base(Filename)
        {
            hasHeaderRow = HasHeaderRow;

            using (var stream = new MemoryStream((int)XlsxStream.Length))
            {
                XlsxStream.CopyTo(stream);

                ParseXlsx(stream);
            }
        }

        public override int RecordCount
        {
            get
            {
                return rawData.Count;
            }
        }

        public override IDeviceImportDataReader GetDataReader()
        {
            return new XlsxDeviceImportDataReader(this, rawData, hasHeaderRow);
        }

        private void ParseXlsx(Stream XlsxStream)
        {
            using (var xlWorkbook = new XLWorkbook(XlsxStream, XLEventTracking.Disabled))
            {
                if (xlWorkbook.Worksheets.Count == 0)
                    throw new IndexOutOfRangeException("Workbook contains no worksheets");

                // Use first worksheet
                var worksheet = xlWorkbook.Worksheets.Worksheet(1);

                SetDatasetName($"{Filename} [Sheet: {worksheet.Name}]");

                var columnCount = worksheet.LastColumnUsed().ColumnNumber();

                if (hasHeaderRow)
                {
                    var headerRow = worksheet.FirstRow();
                    SetColumns(Enumerable.Range(1, columnCount)
                        .Select(i =>
                        {
                            var cell = headerRow.Cell(i);
                            var headerName = cell.GetString();
                            if (string.IsNullOrWhiteSpace(headerName))
                            {
                                headerName = $"Column {cell.WorksheetColumn().ColumnLetter()}";
                            }
                            return new DeviceImportColumn()
                            {
                                Index = i - 1,
                                Name = headerName,
                                Type = DeviceImportFieldTypes.IgnoreColumn
                            };
                        }));
                }
                else
                {
                    SetColumns(Enumerable.Range(1, columnCount)
                        .Select(i => new DeviceImportColumn()
                        {
                            Index = i - 1,
                            Name = $"Column {worksheet.Column(i).ColumnLetter()}",
                            Type = DeviceImportFieldTypes.IgnoreColumn
                        }));
                }

                // Import Data
                var rawData = new List<object[]>();
                foreach (var row in worksheet.RowsUsed().Skip(hasHeaderRow ? 1 : 0))
                {
                    var record = new object[columnCount];
                    rawData.Add(record);

                    for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
                    {
                        var cell = row.Cell(columnIndex + 1);
                        var cellValue = cell.Value;

                        switch (cell.DataType)
                        {
                            case XLCellValues.Number:
                                if (cellValue is double)
                                {
                                    record[columnIndex] = (double)cellValue;
                                }
                                continue;
                            case XLCellValues.Boolean:
                                if (cellValue is bool)
                                {
                                    record[columnIndex] = (bool)cellValue;
                                }
                                continue;
                            case XLCellValues.DateTime:
                                if (cellValue is DateTime)
                                {
                                    record[columnIndex] = (DateTime)cellValue;
                                }
                                continue;
                        }

                        var stringValue = cellValue == null ? null : cellValue.ToString();
                        if (stringValue != null && stringValue.Length == 0)
                        {
                            stringValue = null;
                        }
                        record[columnIndex] = stringValue;
                    }
                }

                this.rawData = rawData;
            }
        }
    }
}
