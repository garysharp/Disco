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

        public override bool Parse(DiscoDataContext Database, IDeviceImportCache Cache, IDeviceImportContext Context, string DeviceSerialNumber, Device ExistingDevice, List<IDeviceImportRecord> PreviousRecords, IDeviceImportDataReader DataReader, int ColumnIndex)
        {
            var value = DataReader.GetString(ColumnIndex);

            // Validate
            if (string.IsNullOrWhiteSpace(value))
                return Error("The Device Serial Number is required");
            else
            {
                parsedValue = value.Trim();
                if (parsedValue.Length > maxLength)
                    return Error($"Cannot be more than {maxLength} characters");
                if (parsedValue.Contains(@"/"))
                    return Error(@"The '/' character is not allowed.");
                if (parsedValue.Contains(@"\"))
                    return Error(@"The '\' character is not allowed.");
            }

            // Duplicate
            var duplicate = PreviousRecords
                .Where(r => IsDeviceSerialNumberValid(r.DeviceSerialNumber))
                .FirstOrDefault(r => r.DeviceSerialNumber.Equals(parsedValue, StringComparison.OrdinalIgnoreCase));
            if (duplicate != null)
                return Error($"This Device Serial Number was already present on Row {DataReader.GetRowNumber(duplicate.Index)}");

            // No action required
            return Success(EntityState.Unchanged);
        }

        public override bool Apply(DiscoDataContext Database, Device Device)
        {
            // Do nothing for Device Serial Number
            return false;
        }

        public override int? GuessColumn(DiscoDataContext Database, IDeviceImportContext Context, IDeviceImportDataReader DataReader)
        {
            // 'serial' in column name
            var possibleColumns = Context.Columns
                .Where(h => h.Type == DeviceImportFieldTypes.IgnoreColumn && h.Name.IndexOf("serial", StringComparison.OrdinalIgnoreCase) >= 0);

            // All Values
            possibleColumns = possibleColumns.Where(h => DataReader.TestAllNotEmpty(h.Index)).ToList();

            if (possibleColumns.Count() > 1)
            {
                // Hunt Database
                var possibleColumnIndex = possibleColumns.Where(h =>
                {
                    try
                    {
                        var top50SerialNumbers = DataReader.GetStrings(h.Index).Take(50).ToList();
                        return Database.Devices.Count(d => top50SerialNumbers.Contains(d.SerialNumber)) > 0;
                    }
                    catch (Exception) { return false; }
                }).Select(h => (int?)h.Index).FirstOrDefault();

                if (possibleColumnIndex.HasValue)
                    return possibleColumnIndex;
            }

            return possibleColumns.Select(h => (int?)h.Index).FirstOrDefault();
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
