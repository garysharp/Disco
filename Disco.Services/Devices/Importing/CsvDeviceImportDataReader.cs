using Disco.Models.Services.Devices.Importing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Disco.Services.Devices.Importing
{
    public class CsvDeviceImportDataReader : IDeviceImportDataReader
    {
        private readonly static string[] TrueValues = { "true", "1", "yes", "-1", "on" };
        private readonly static string[] FalseValues = { "false", "0", "no", "off" };

        private readonly List<string[]> rawData;
        private int currentRowIndex;
        private readonly int rowOffset;
        private string[] currentRow;

        public int Index { get { return currentRowIndex; } }

        public CsvDeviceImportDataReader(List<string[]> RawData, bool HasHeaderRow)
        {
            rawData = RawData;
            currentRowIndex = -1;
            rowOffset = HasHeaderRow ? 1 : 0;
        }

        public void Reset()
        {
            currentRowIndex = 0;
            currentRow = null;
        }

        public bool Read()
        {
            if (++currentRowIndex >= rawData.Count)
            {
                currentRowIndex--;
                return false;
            }

            currentRow = rawData[currentRowIndex];
            return true;
        }

        public int GetRowNumber(int index)
        {
            return index + rowOffset;
        }

        public IDeviceImportValueReader GetValue(int columnIndex)
        {
            if (currentRow == null)
                throw new InvalidOperationException($"{nameof(Read)} must be called before retrieving values");

            return new CsvDeviceImportValueReader(currentRow[columnIndex]);
        }

        public bool TestAllNotEmpty(int columnIndex)
        {
            return rawData.Select(r => r[columnIndex])
                .All(s => !string.IsNullOrWhiteSpace(s));
        }

        public bool TestAllNullableInt(int columnIndex)
        {
            return rawData.Select(r => r[columnIndex])
                .All(c =>
                {
                    return CsvDeviceImportValueReader.TryGetNullableInt(c, out _);
                });
        }

        public bool TestAllInt(int columnIndex)
        {
            return rawData.Select(r => r[columnIndex])
                .All(c =>
                {
                    return CsvDeviceImportValueReader.TryGetNullableInt(c, out var value) && value.HasValue;
                });
        }

        public bool TestAllNullableBool(int columnIndex)
        {
            return rawData.Select(r => r[columnIndex])
                .All(c =>
                {
                    return CsvDeviceImportValueReader.TryGetNullableBool(c, out _);
                });
        }

        public bool TestAllNullableDateTime(int columnIndex)
        {
            return rawData.Select(r => r[columnIndex])
                .All(c =>
                {
                    return CsvDeviceImportValueReader.TryGetNullableDateTime(c, out _);
                });
        }

        private sealed class CsvDeviceImportValueReader : IDeviceImportValueReader
        {
            private readonly string columnValue;

            public CsvDeviceImportValueReader(string columnValue)
            {
                this.columnValue = columnValue;
            }

            public string GetString()
            {
                if (columnValue.Length == 0)
                    return null;

                return columnValue;
            }

            public bool TryGetNullableInt(out int? value)
                => TryGetNullableInt(columnValue, out value);

            public bool TryGetNullableBool(out bool? value)
                => TryGetNullableBool(columnValue, out value);

            public bool TryGetNullableDateTime(out DateTime? value)
                => TryGetNullableDateTime(columnValue, out value);

            internal static bool TryGetNullableDateTime(string content, out DateTime? value)
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    value = null;
                    return true;
                }
                else
                {
                    content = content.Trim();

                    if (DateTime.TryParse(content, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out var valueDateTime))
                    {
                        value = valueDateTime;
                        return true;
                    }
                    else
                    {
                        value = null;
                        return false;
                    }
                }
            }

            internal static bool TryGetNullableBool(string content, out bool? value)
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    value = null;
                    return true;
                }
                else
                {
                    content = content.Trim();

                    if (TrueValues.Contains(content, StringComparer.OrdinalIgnoreCase))
                    {
                        value = true;
                        return true;
                    }
                    else if (FalseValues.Contains(content, StringComparer.OrdinalIgnoreCase))
                    {
                        value = false;
                        return true;
                    }
                    else
                    {
                        value = null;
                        return false;
                    }
                }
            }

            internal static bool TryGetNullableInt(string content, out int? value)
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    value = null;
                    return true;
                }
                else
                {
                    if (int.TryParse(content, out var intValue))
                    {
                        value = intValue;
                        return true;
                    }
                    else
                    {
                        value = null;
                        return false;
                    }
                }
            }
        }
    }
}
