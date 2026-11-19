using Disco.Models.Services.Devices.Importing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Disco.Services.Devices.Importing
{
    public class XlsxDeviceImportDataReader : IDeviceImportDataReader
    {
        private readonly static string[] TrueValues = { "true", "1", "yes", "-1", "on" };
        private readonly static string[] FalseValues = { "false", "0", "no", "off" };

        private readonly List<object[]> rawData;
        private int currentRowIndex;
        private readonly int rowOffset;
        private object[] currentRow;

        public int Index { get { return currentRowIndex; } }

        public XlsxDeviceImportDataReader(List<object[]> rawData, bool hasHeaderRow)
        {
            this.rawData = rawData;
            currentRowIndex = -1;
            rowOffset = hasHeaderRow ? 2 : 1;
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

        public int GetRowNumber(int index)
        {
            return index + rowOffset;
        }


        public IDeviceImportValueReader GetValue(int columnIndex)
        {
            if (currentRow == null)
                throw new InvalidOperationException($"{nameof(Read)} must be called before retrieving values");

            return new XlsxDeviceImportValueReader(currentRow[columnIndex]);
        }

        public bool TestAllNotEmpty(int columnIndex)
        {
            return rawData.Select(r => r[columnIndex]?.ToString())
                .All(s => !string.IsNullOrWhiteSpace(s));
        }

        public bool TestAllNullableInt(int columnIndex)
        {
            return rawData.Select(r => r[columnIndex])
                .All(c =>
                {
                    return XlsxDeviceImportValueReader.TryGetNullableInt(c, out _);
                });
        }

        public bool TestAllInt(int columnIndex)
        {
            return rawData.Select(r => r[columnIndex])
                .All(c =>
                {
                    return XlsxDeviceImportValueReader.TryGetNullableInt(c, out var value) && value.HasValue;
                });
        }

        public bool TestAllNullableBool(int columnIndex)
        {
            return rawData.Select(r => r[columnIndex])
                .All(c =>
                {
                    return XlsxDeviceImportValueReader.TryGetNullableBool(c, out _);
                });
        }

        public bool TestAllNullableDateTime(int columnIndex)
        {
            return rawData.Select(r => r[columnIndex])
                .All(c =>
                {
                    return XlsxDeviceImportValueReader.TryGetNullableDateTime(c, out _);
                });
        }

        private sealed class XlsxDeviceImportValueReader : IDeviceImportValueReader
        {
            private readonly object columnValue;

            public XlsxDeviceImportValueReader(object columnValue)
            {
                this.columnValue = columnValue;
            }

            public string GetString()
            {
                if (columnValue == null)
                    return null;
                else
                    return columnValue.ToString();
            }

            public bool TryGetNullableInt(out int? value)
                => TryGetNullableInt(columnValue, out value);

            public bool TryGetNullableBool(out bool? value)
                => TryGetNullableBool(columnValue, out value);

            public bool TryGetNullableDateTime(out DateTime? value)
                => TryGetNullableDateTime(columnValue, out value);

            internal static bool TryGetNullableDateTime(object Content, out DateTime? value)
            {
                if (Content == null)
                {
                    value = null;
                    return true;
                }

                if (Content is DateTime dateTimeContent)
                {
                    value = dateTimeContent;
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

            internal static bool TryGetNullableBool(object content, out bool? value)
            {
                if (content == null)
                {
                    value = null;
                    return true;
                }

                if (content is bool boolContent)
                {
                    value = boolContent;
                    return true;
                }

                var stringValue = content.ToString();

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

            internal static bool TryGetNullableInt(object content, out int? value)
            {
                if (content == null)
                {
                    value = null;
                    return true;
                }

                if (content is double doubleContent)
                {
                    value = (int)doubleContent;
                    return true;
                }

                var stringValue = content.ToString();

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
}
