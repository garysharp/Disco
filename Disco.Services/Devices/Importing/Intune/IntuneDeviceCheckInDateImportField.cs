using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using System;
using System.Data;

namespace Disco.Services.Devices.Importing.Intune
{
    internal class IntuneDeviceCheckInDateImportField : DeviceImportFieldBase
    {
        private DateTime? parsedValue;
        private DateTime? previousValue;

        public override DeviceImportFieldTypes FieldType => DeviceImportFieldTypes.IgnoreColumn;

        public override object RawParsedValue => parsedValue;
        public override string FriendlyValue => parsedValue?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A";
        public override string FriendlyPreviousValue => previousValue?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A";

        public override bool Parse(DeviceImportDatabaseParsingState state, Device existingDevice, IDeviceImportValueReader valueReader)
        {
            previousValue = existingDevice?.LastNetworkLogonDate;

            valueReader.TryGetNullableDateTime(out parsedValue);

            if (parsedValue == null)
                return Success(EntityState.Unchanged);

            var transformed = parsedValue.Value.ToLocalTime();
            parsedValue = new DateTime((transformed.Ticks / TimeSpan.TicksPerSecond) * TimeSpan.TicksPerSecond, DateTimeKind.Local);

            if (previousValue.HasValue)
            {
                // only update if the value is newer
                if (previousValue.Value.Ticks / TimeSpan.TicksPerSecond == parsedValue.Value.Ticks / TimeSpan.TicksPerSecond ||
                    previousValue.Value > parsedValue.Value)
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
                device.LastNetworkLogonDate = parsedValue;
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
