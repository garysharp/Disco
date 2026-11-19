using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using Disco.Services.Interop.ActiveDirectory;
using System;
using System.Data;

namespace Disco.Services.Devices.Importing.Intune
{
    internal class IntuneDeviceComputerNameImportField : DeviceImportFieldBase
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
            previousValue = existingDevice?.DeviceDomainId;
            var value = valueReader.GetString();

            if (string.IsNullOrWhiteSpace(value))
                parsedValue = null;
            else
                parsedValue = value.Trim();

            if (parsedValue == null)
                return Success(EntityState.Unchanged);

            if (parsedValue.Length > maxLength)
                return Error($"Cannot be more than {maxLength} characters");

            if (existingDevice != null)
            {
                if (ActiveDirectory.IsValidDomainAccountId(existingDevice.DeviceDomainId, out var existingName, out var existingDomain))
                    parsedValue = $@"{existingDomain.NetBiosName}\{parsedValue}";

                if (string.Equals(parsedValue, existingDevice.DeviceDomainId, StringComparison.OrdinalIgnoreCase))
                    return Success(EntityState.Unchanged);

                return Success(EntityState.Modified);
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
            => throw new NotImplementedException();
    }
}
