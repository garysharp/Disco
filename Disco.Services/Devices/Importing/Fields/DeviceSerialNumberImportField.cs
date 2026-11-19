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

        public override DeviceImportFieldTypes FieldType => DeviceImportFieldTypes.DeviceSerialNumber;

        public override object RawParsedValue => parsedValue;
        public override string FriendlyValue => parsedValue;
        public override string FriendlyPreviousValue => parsedValue;

        public static DeviceSerialNumberImportField Create(Device device)
        {
            var field = new DeviceSerialNumberImportField()
            {
                parsedValue = device.SerialNumber,
            };
            field.Success(EntityState.Unchanged);
            return field;
        }

        public override bool Parse(DeviceImportDatabaseParsingState state, Device existingDevice, IDeviceImportValueReader valueReader)
        {
            var value = valueReader.GetString();

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

            // No action required
            return Success(EntityState.Unchanged);
        }

        public override bool Parsed(DeviceImportDatabaseParsingState state, Device existingDevice, IEnumerable<IDeviceImportRecord> records, IDeviceImportRecord record)
        {
            var duplicate = records
                .TakeWhile(r => r != record)
                .FirstOrDefault(r => string.Equals(parsedValue, r.DeviceSerialNumber, StringComparison.OrdinalIgnoreCase));

            if (duplicate != null)
                return Error($"The Device Serial Number was already present on Row {duplicate.Index}");

            return true;
        }

        public override bool Apply(DiscoDataContext database, Device device)
        {
            // Do nothing for Device Serial Number
            return false;
        }

        public override int? GuessColumn(IDeviceImportContext context, IDeviceImportDataReader dataReader)
        {
            // 'serial' in column name
            var possibleColumns = context.Columns
                .Where(h => h.Type == DeviceImportFieldTypes.IgnoreColumn && h.Name.IndexOf("serial", StringComparison.OrdinalIgnoreCase) >= 0);

            // All Values
            possibleColumns = possibleColumns.Where(h => dataReader.TestAllNotEmpty(h.Index)).ToList();

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
