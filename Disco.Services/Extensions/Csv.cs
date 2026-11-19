using LumenWorks.Framework.IO.Csv;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Disco.Services.Extensions
{
    internal static class Csv
    {
        public static IEnumerable<T> Parse<T>(Stream stream) where T : CsvRecord, new()
        {
            using (var reader = new StreamReader(stream))
            {
                using (var csvReader = new CsvReader(reader, true))
                {
                    csvReader.DefaultParseErrorAction = ParseErrorAction.ThrowException;
                    csvReader.MissingFieldAction = MissingFieldAction.ReplaceByNull;

                    var propertyMapper = BuildMapper<T>(csvReader);

                    while (csvReader.ReadNextRecord())
                    {
                        var record = new T
                        {
                            RowNumber = (int)csvReader.CurrentRecordIndex + 1
                        };

                        foreach (var map in propertyMapper)
                        {
                            var value = csvReader[map.Index];
                            if (string.IsNullOrEmpty(value))
                            {
                                if (!map.CanAssignNull)
                                    throw new FormatException($"CSV value for header '{map.Header}' on row {record.RowNumber} cannot be empty for type {map.Property.PropertyType.FullName}.");

                                map.Property.SetValue(record, null);
                            }
                            else
                            {
                                var convertedValue = map.Converter(value);
                                map.Property.SetValue(record, convertedValue);
                            }
                        }

                        yield return record;
                    }
                }
            }
        }

        private static List<PropertyMapper> BuildMapper<T>(CsvReader csvReader)
        {
            var headers = csvReader.GetFieldHeaders();

            // `T` contains public properties with [Display] attributes where the Name property matches the CSV headers. Use reflection to map the CSV fields to the properties of `T`.
            // build a dictionary of property names to PropertyInfo objects for the type `T`
            var propertyMapper = new List<PropertyMapper>(headers.Length);
            foreach (var property in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var columnName = property.GetCustomAttribute<DisplayAttribute>(false)?.Name
                    ?? property.GetCustomAttribute<DisplayNameAttribute>(false)?.DisplayName;
                if (columnName != null)
                {
                    var index = Array.IndexOf(headers, columnName);
                    if (index < 0)
                        throw new InvalidOperationException($"CSV header '{columnName}' not found in the CSV file.");

                    var converter = GetConverter(property);

                    propertyMapper.Add(new PropertyMapper(index, columnName, converter, property));
                }
            }

            return propertyMapper;
        }

        private static Func<string, object> GetConverter(PropertyInfo property)
        {
            var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            if (propertyType == typeof(string))
                return s => s;
            if (propertyType == typeof(int))
                return s => int.Parse(s);
            if (propertyType == typeof(double))
                return s => double.Parse(s);
            if (propertyType == typeof(bool))
                return s => bool.Parse(s);
            if (propertyType == typeof(DateTime))
                return s => DateTime.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
            if (propertyType == typeof(long))
                return s => long.Parse(s, CultureInfo.InvariantCulture);
            throw new NotSupportedException($"No converter available for {property.DeclaringType.FullName}.{property.Name} type {property.PropertyType.FullName}");
        }

        private sealed class PropertyMapper
        {
            public int Index { get; }
            public string Header { get; }
            public Func<string, object> Converter { get; }
            public PropertyInfo Property { get; }
            public bool CanAssignNull { get; }

            public PropertyMapper(int index, string header, Func<string, object> converter, PropertyInfo property)
            {
                Index = index;
                Header = header;
                Converter = converter;
                Property = property;
                CanAssignNull = !property.PropertyType.IsValueType || Nullable.GetUnderlyingType(property.PropertyType) != null;
            }
        }

        internal abstract class CsvRecord
        {
            public int RowNumber { get; internal set; }
        }
    }
}
