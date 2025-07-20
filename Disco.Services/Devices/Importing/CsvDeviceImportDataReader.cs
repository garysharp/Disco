using Disco.Models.Services.Devices.Importing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Disco.Services.Devices.Importing
{
    public class CsvDeviceImportDataReader : IDeviceImportDataReader
    {
        private static string[] TrueValues = { "true", "1", "yes", "-1", "on" };
        private static string[] FalseValues = { "false", "0", "no", "off" };

        private CsvDeviceImportContext context;
        private List<string[]> rawData;
        private int currentRowIndex;
        private int rowOffset;
        private string[] currentRow;

        public int Index { get { return currentRowIndex; } }

        public CsvDeviceImportDataReader(CsvDeviceImportContext Context, List<string[]> RawData, bool HasHeaderRow)
        {
            context = Context;
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

        public int GetRowNumber(int Index)
        {
            return Index + rowOffset;
        }

        public string GetString(int ColumnIndex)
        {
            if (currentRow == null)
                throw new InvalidOperationException($"{nameof(CsvDeviceImportDataReader.Read)} must be called before retrieving values");

            var value = currentRow[ColumnIndex];

            if (value.Length == 0)
                return null;

            return value;
        }

        public IEnumerable<string> GetStrings(int ColumnIndex)
        {
            return rawData.Select(r => r[ColumnIndex]);
        }

        public bool TryGetNullableInt(int ColumnIndex, out int? value)
        {
            if (currentRow == null)
                throw new InvalidOperationException($"{nameof(CsvDeviceImportDataReader.Read)} must be called before retrieving values");

            return TryGetNullableInt(currentRow[ColumnIndex], out value);
        }

        public bool TryGetNullableBool(int ColumnIndex, out bool? value)
        {
            if (currentRow == null)
                throw new InvalidOperationException($"{nameof(CsvDeviceImportDataReader.Read)} must be called before retrieving values");

            return TryGetNullableBool(currentRow[ColumnIndex], out value);
        }

        public bool TryGetNullableDateTime(int ColumnIndex, out DateTime? value)
        {
            if (currentRow == null)
                throw new InvalidOperationException($"{nameof(CsvDeviceImportDataReader.Read)} must be called before retrieving values");

            return TryGetNullableDateTime(currentRow[ColumnIndex], out value);
        }

        public bool TestAllNotEmpty(int ColumnIndex)
        {
            return GetStrings(ColumnIndex).All(s => !string.IsNullOrWhiteSpace(s));
        }

        public bool TestAllNullableInt(int ColumnIndex)
        {
            return rawData.Select(r => r[ColumnIndex])
                .All(c =>
                {
                    return TryGetNullableInt(c, out var value);
                });
        }

        public bool TestAllInt(int ColumnIndex)
        {
            return rawData.Select(r => r[ColumnIndex])
                .All(c =>
                {
                    return TryGetNullableInt(c, out var value) && value.HasValue;
                });
        }

        public bool TestAllNullableBool(int ColumnIndex)
        {
            return rawData.Select(r => r[ColumnIndex])
                .All(c =>
                {
                    return TryGetNullableBool(c, out var value);
                });
        }

        public bool TestAllNullableDateTime(int ColumnIndex)
        {
            return rawData.Select(r => r[ColumnIndex])
                .All(c =>
                {
                    return TryGetNullableDateTime(c, out var value);
                });
        }

        public void Dispose()
        {
            // Nothing to dispose
        }

        private bool TryGetNullableDateTime(string content, out DateTime? value)
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

        private bool TryGetNullableBool(string content, out bool? value)
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

        private bool TryGetNullableInt(string content, out int? value)
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