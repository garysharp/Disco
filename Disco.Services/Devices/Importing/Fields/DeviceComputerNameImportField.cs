using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using Disco.Services.Interop.ActiveDirectory;
using System;
using System.Data;
using System.Linq;

namespace Disco.Services.Devices.Importing.Fields
{
    internal class DeviceComputerNameImportField : DeviceImportFieldBase
    {
        private const int maxLength = 1024;
        private string parsedValue;
        private string previousValue;

        public override DeviceImportFieldTypes FieldType => DeviceImportFieldTypes.DeviceComputerName;

        public override object RawParsedValue => parsedValue;
        public override string FriendlyValue => parsedValue;
        public override string FriendlyPreviousValue => previousValue;

        public override bool Parse(DeviceImportDatabaseParsingState state, Device existingDevice, IDeviceImportValueReader valueReader)
        {
            var value = valueReader.GetString();

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

            if (existingDevice != null)
            {
                if (string.Equals(parsedValue, existingDevice.DeviceDomainId, StringComparison.OrdinalIgnoreCase))
                    return Success(EntityState.Unchanged);
                if (existingDevice.EnrolledDate.HasValue)
                    return Error("The Device Computer Name cannot be changed after a device has enrolled");
                else
                {
                    previousValue = existingDevice.DeviceDomainId;
                    return Success(EntityState.Modified);
                }
            }

            return Success(EntityState.Added);
        }

        public override bool Apply(DiscoDataContext database, Device device)
        {
            if (FieldAction == EntityState.Added ||
                FieldAction == EntityState.Modified)
            {
                device.DeviceDomainId = parsedValue;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override int? GuessColumn(IDeviceImportContext context, IDeviceImportDataReader dataReader)
        {
            // column name
            var possibleColumns = context.Columns
                .Where(h => h.Type == DeviceImportFieldTypes.IgnoreColumn &&
                    h.Name.IndexOf("name", StringComparison.OrdinalIgnoreCase) >= 0);

            return possibleColumns.Select(h => (int?)h.Index).FirstOrDefault();
        }
    }
}
