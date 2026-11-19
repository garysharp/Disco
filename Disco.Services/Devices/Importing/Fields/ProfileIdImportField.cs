using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Logging;
using System;
using System.Data;
using System.Linq;

namespace Disco.Services.Devices.Importing.Fields
{
    internal class ProfileIdImportField : DeviceImportFieldBase
    {
        private int parsedValue;
        private string friendlyValue;
        private string friendlyPreviousValue;

        public override DeviceImportFieldTypes FieldType { get { return DeviceImportFieldTypes.ProfileId; } }

        public override object RawParsedValue { get { return parsedValue; } }
        public override string FriendlyValue { get { return friendlyValue; } }
        public override string FriendlyPreviousValue { get { return friendlyPreviousValue; } }

        public override bool Parse(DeviceImportDatabaseParsingState state, Device existingDevice, IDeviceImportValueReader valueReader)
        {
            if (valueReader.TryGetNullableInt(out var intValue))
            {
                if (!intValue.HasValue)
                {
                    if (existingDevice == null)
                    {
                        intValue = state.Database.DiscoConfiguration.DeviceProfiles.DefaultAddDeviceOfflineDeviceProfileId; // Default Model for new devices
                    }
                    else
                    {
                        return Error("The Profile Identifier cannot be blank");
                    }
                }

                parsedValue = intValue.Value;
                friendlyValue = parsedValue.ToString();
            }
            else
            {
                return Error("The Profile Identifier must be a number");
            }

            var p = state.DeviceProfiles.FirstOrDefault(dp => dp.Id == parsedValue);

            if (p == null)
                return Error($"The identifier ({parsedValue}) does not match any Device Profile");

            friendlyValue = $"{p.Description} [{p.Id}]";

            if (existingDevice == null)
                return Success(EntityState.Added);
            else if (existingDevice != null && existingDevice.DeviceProfileId != parsedValue)
            {
                var previousProfile = state.DeviceProfiles.FirstOrDefault(dp => dp.Id == existingDevice.DeviceProfileId);
                friendlyPreviousValue = $"{previousProfile.Description} [{previousProfile.Id}]";

                return Success(EntityState.Modified);
            }
            else
                return Success(EntityState.Unchanged);
        }

        public override bool Apply(DiscoDataContext database, Device device)
        {
            if (FieldAction == EntityState.Added ||
                FieldAction == EntityState.Modified)
            {
                device.DeviceProfileId = parsedValue;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Applied(DiscoDataContext database, Device device, ref bool deviceADDescriptionSet)
        {
            if (!deviceADDescriptionSet && ActiveDirectory.IsValidDomainAccountId(device.DeviceDomainId))
            {
                var adAccount = device.ActiveDirectoryAccount();

                if (adAccount != null && !adAccount.IsCriticalSystemObject)
                {
                    try
                    {
                        adAccount.SetDescription(device);
                    }
                    catch (Exception ex)
                    {
                        SystemLog.LogWarning($"Unable to update AD Machine Account Description for {device.DeviceDomainId}: {ex.Message}");
                        throw;
                    }
                    deviceADDescriptionSet = true;
                }
            }
        }

        public override int? GuessColumn(IDeviceImportContext context, IDeviceImportDataReader dataReader)
        {
            // column name
            var possibleColumns = context.Columns
                .Where(h => h.Type == DeviceImportFieldTypes.IgnoreColumn &&
                    h.Name.IndexOf("profile", StringComparison.OrdinalIgnoreCase) >= 0);

            // All Integers Numbers
            possibleColumns = possibleColumns.Where(h => dataReader.TestAllInt(h.Index)).ToList();

            // Multiple Columns, tighten column definition
            if (possibleColumns.Count() > 1)
            {
                possibleColumns = possibleColumns
                    .Where(h =>
                        h.Name.IndexOf("profileid", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        h.Name.IndexOf("profile id", StringComparison.OrdinalIgnoreCase) >= 0);
            }

            return possibleColumns.Select(h => (int?)h.Index).FirstOrDefault();
        }
    }
}
