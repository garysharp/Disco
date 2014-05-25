using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Disco.Services.Devices.Importing.Fields
{
    internal class DeviceSerialNumberImportField : DeviceImportFieldBase
    {
        private const int maxLength = 60;
        private string parsedValue;

        public override DeviceImportFieldTypes FieldType { get { return DeviceImportFieldTypes.DeviceSerialNumber; } }

        public override object RawParsedValue { get { return parsedValue; } }
        public override string FriendlyValue { get { return parsedValue; } }
        public override string FriendlyPreviousValue { get { return parsedValue; } }

        public override bool Parse(DiscoDataContext Database, IDeviceImportCache Cache, DeviceImportContext Context, int RecordIndex, string DeviceSerialNumber, Device ExistingDevice, Dictionary<DeviceImportFieldTypes, string> Values, string Value)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(Value))
                return Error("The Device Serial Number is required");
            else
            {
                parsedValue = Value.Trim();
                if (parsedValue.Length > maxLength)
                    return Error(string.Format("Cannot be more than {0} characters", maxLength));
            }

            // Duplicate
            var duplicate = Context.RawData
                .Take(RecordIndex)
                .Select((r, i) => Tuple.Create(i, ParseRawDeviceSerialNumber(r[Context.HeaderDeviceSerialNumberIndex])))
                .Where(r => IsDeviceSerialNumberValid(r.Item2))
                .FirstOrDefault(r => r.Item2.Equals(parsedValue, StringComparison.OrdinalIgnoreCase));
            if (duplicate != null)
                return Error(string.Format("This Device Serial Number was already present on Row {0}", duplicate.Item1 + 1));

            // No action required
            return Success(EntityState.Unchanged);
        }

        public override bool Apply(DiscoDataContext Database, Device Device)
        {
            // Do nothing for Device Serial Number
            return false;
        }

        public override int? GuessHeader(DiscoDataContext Database, DeviceImportContext Context)
        {
            // 'serial' in column name
            var possibleColumns = Context.Header
                .Select((h, i) => Tuple.Create(h, i))
                .Where(h => h.Item1.Item2 == DeviceImportFieldTypes.IgnoreColumn && h.Item1.Item1.IndexOf("serial", System.StringComparison.OrdinalIgnoreCase) >= 0);

            // All Values
            possibleColumns = possibleColumns.Where(h =>
            {
                return Context.RawData.Select(v => v[h.Item2]).All(v => !string.IsNullOrWhiteSpace(v));
            }).ToList();

            if (possibleColumns.Count() > 1)
            {
                // Hunt Database
                var possibleColumnIndex = possibleColumns.Where(h =>
                {
                    try
                    {
                        var top50SerialNumbers = Context.RawData.Select(v => v[h.Item2]).Take(50).ToArray();
                        return Database.Devices.Count(d => top50SerialNumbers.Contains(d.SerialNumber)) > 0;
                    }
                    catch (Exception) { return false; }
                }).Select(h => (int?)h.Item2).FirstOrDefault();

                if (possibleColumnIndex.HasValue)
                    return possibleColumnIndex;
            }

            return possibleColumns.Select(h => (int?)h.Item2).FirstOrDefault();
        }

        public static string ParseRawDeviceSerialNumber(string DeviceSerialNumber)
        {
            if (string.IsNullOrWhiteSpace(DeviceSerialNumber))
                return null;
            else
                return DeviceSerialNumber.Trim();
        }
        public static bool IsDeviceSerialNumberValid(string DeviceSerialNumber)
        {
            return DeviceSerialNumber != null && DeviceSerialNumber.Length <= maxLength;
        }
    }
}
