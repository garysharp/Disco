using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using Disco.Services.Interop.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Disco.Services.Devices.Importing.Fields
{
    internal class DeviceComputerNameImportField : DeviceImportFieldBase
    {
        private const int maxLength = 60;
        private string parsedValue;
        private string previousValue;

        public override DeviceImportFieldTypes FieldType { get { return DeviceImportFieldTypes.DeviceComputerName; } }

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

            if (string.IsNullOrEmpty(parsedValue))
                return Error("The Device Computer Name is required");

            try
            {
                if (ActiveDirectory.IsValidDomainAccountId(parsedValue, out var accountUsername, out var accountDomain))
                    parsedValue = $@"{accountDomain.NetBiosName}\{accountUsername}";
                else
                    return Error(@"The expected format is 'DOMAIN\ComputerName'");
            }
            catch (ArgumentException ex) when (ex.ParamName == "NetBiosName")
            {
                return Error(ex.Message);
            }

            if (parsedValue.Length > maxLength)
                return Error($"Cannot be more than {maxLength} characters");

            if (ExistingDevice != null)
            {
                if (string.Equals(parsedValue, ExistingDevice.DeviceDomainId, StringComparison.OrdinalIgnoreCase))
                    return Success(EntityState.Unchanged);
                if (ExistingDevice.EnrolledDate.HasValue)
                    return Error("The Device Computer Name cannot be changed after a device has enrolled");
                else
                {
                    previousValue = ExistingDevice.DeviceDomainId;
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
                Device.DeviceDomainId = parsedValue;
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
                    h.Name.IndexOf("name", StringComparison.OrdinalIgnoreCase) >= 0);

            return possibleColumns.Select(h => (int?)h.Index).FirstOrDefault();
        }
    }
}
