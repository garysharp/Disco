using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Disco.Services.Devices.Importing.Fields
{
    internal class DeviceDnsHostNameImportField : DeviceImportFieldBase
    {
        private const int maxLength = 2048;
        private string parsedValue;
        private string previousValue;

        public override DeviceImportFieldTypes FieldType { get { return DeviceImportFieldTypes.DeviceDnsHostName; } }

        public override object RawParsedValue { get { return parsedValue; } }
        public override string FriendlyValue { get { return parsedValue; } }
        public override string FriendlyPreviousValue { get { return previousValue; } }

        public override bool Parse(DiscoDataContext Database, IDeviceImportCache Cache, IDeviceImportContext Context, string DeviceSerialNumber, Device ExistingDevice, List<IDeviceImportRecord> PreviousRecords, IDeviceImportDataReader DataReader, int ColumnIndex)
        {
            var value = DataReader.GetString(ColumnIndex);

            if (string.IsNullOrWhiteSpace(value))
                parsedValue = null;
            else
                parsedValue = value.Trim();

            if (parsedValue.Length > maxLength)
                return Error($"Cannot be more than {maxLength} characters");

            if (parsedValue.IndexOfAny(new char[] { ' ', '\t', '\r', '\n' }) >= 0)
                return Error("Cannot contain whitespace characters");

            if (parsedValue.IndexOf('.') >= 0)
                return Error("Cannot contain a period character");

            if (ExistingDevice != null)
            {
                if (string.Equals(parsedValue, ExistingDevice.DnsHostName, StringComparison.OrdinalIgnoreCase))
                    return Success(EntityState.Unchanged);
                if (ExistingDevice.EnrolledDate.HasValue)
                    return Error("The Device DNS Host Name cannot be changed after a device has enrolled");
                else
                {
                    previousValue = ExistingDevice.DnsHostName;
                    return Success(EntityState.Modified);
                }
            }

            return Success(EntityState.Added);
        }

        public override bool Apply(DiscoDataContext Database, Device Device)
        {
            if (FieldAction == EntityState.Added ||
                FieldAction == EntityState.Modified)
            {
                Device.DnsHostName = parsedValue;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override int? GuessColumn(DiscoDataContext Database, IDeviceImportContext Context, IDeviceImportDataReader DataReader)
        {
            // column name
            var possibleColumns = Context.Columns
                .Where(h => h.Type == DeviceImportFieldTypes.IgnoreColumn &&
                    h.Name.IndexOf("dns", StringComparison.OrdinalIgnoreCase) >= 0);

            return possibleColumns.Select(h => (int?)h.Index).FirstOrDefault();
        }
    }
}
