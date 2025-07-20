using Disco.Models.Services.Devices.Importing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Disco.Services.Devices.Importing
{
    public class XlsxDeviceImportDataReader : IDeviceImportDataReader
    {
        private static string[] TrueValues = { "true", "1", "yes", "-1", "on" };
        private static string[] FalseValues = { "false", "0", "no", "off" };

        private XlsxDeviceImportContext context;
        private List<object[]> rawData;
        private int currentRowIndex;
        private int rowOffset;
        private object[] currentRow;

        public int Index { get { return currentRowIndex; } }

        public XlsxDeviceImportDataReader(XlsxDeviceImportContext Context, List<object[]> RawData, bool HasHeaderRow)
        {
            context = Context;
            rawData = RawData;
            currentRowIndex = -1;
            rowOffset = HasHeaderRow ? 2 : 1;
        }

        public void Reset()
        {
            currentRowIndex = -1;
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
                throw new InvalidOperationException($"{nameof(Read)} must be called before retrieving values");

            var cell = currentRow[ColumnIndex];

            if (cell == null)
                return null;
            else
                return cell.ToString();
        }

        public IEnumerable<string> GetStrings(int ColumnIndex)
        {
            return rawData.Select(r => r[ColumnIndex]?.ToString());
        }

        public bool TryGetNullableInt(int ColumnIndex, out int? value)
        {
            if (currentRow == null)
                throw new InvalidOperationException($"{nameof(Read)} must be called before retrieving values");

            return TryGetNullableInt(currentRow[ColumnIndex], out value);
        }

        public bool TryGetNullableBool(int ColumnIndex, out bool? value)
        {
            if (currentRow == null)
                throw new InvalidOperationException($"{nameof(Read)} must be called before retrieving values");

            return TryGetNullableBool(currentRow[ColumnIndex], out value);
        }

        public bool TryGetNullableDateTime(int ColumnIndex, out DateTime? value)
        {
            if (currentRow == null)
                throw new InvalidOperationException($"{nameof(Read)} must be called before retrieving values");

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

        private bool TryGetNullableDateTime(object Content, out DateTime? value)
        {
            if (Content == null)
            {
                value = null;
                return true;
            }

            if (Content is DateTime)
            {
                value = (DateTime)Content;
                return true;
            }

            var stringValue = Content.ToString();

            if (string.IsNullOrWhiteSpace(stringValue))
            {
                value = null;
                return true;
            }
            else
            {
                stringValue = stringValue.Trim();

                if (DateTime.TryParse(stringValue, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out var valueDateTime))
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

        private bool TryGetNullableBool(object Content, out bool? value)
        {
            if (Content == null)
            {
                value = null;
                return true;
            }

            if (Content is bool)
            {
                value = (bool)Content;
                return true;
            }

            var stringValue = Content.ToString();

            if (string.IsNullOrWhiteSpace(stringValue))
            {
                value = null;
                return true;
            }
            else
            {
                stringValue = stringValue.Trim();

                if (TrueValues.Contains(stringValue, StringComparer.OrdinalIgnoreCase))
                {
                    value = true;
                    return true;
                }
                else if (FalseValues.Contains(stringValue, StringComparer.OrdinalIgnoreCase))
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

        private bool TryGetNullableInt(object Content, out int? value)
        {
            if (Content == null)
            {
                value = null;
                return true;
            }

            if (Content is double)
            {
                value = (int)((double)Content);
                return true;
            }

            var stringValue = Content.ToString();

            if (string.IsNullOrWhiteSpace(stringValue))
            {
                value = null;
                return true;
            }
            else
            {
                if (int.TryParse(stringValue, out var intValue))
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
