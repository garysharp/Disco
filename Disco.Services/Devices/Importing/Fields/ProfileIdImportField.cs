using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Logging;
using System;
using System.Collections.Generic;
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

        public override bool Parse(DiscoDataContext Database, IDeviceImportCache Cache, IDeviceImportContext Context, string DeviceSerialNumber, Device ExistingDevice, List<IDeviceImportRecord> PreviousRecords, IDeviceImportDataReader DataReader, int ColumnIndex)
        {
            int? intValue;
            if (DataReader.TryGetNullableInt(ColumnIndex, out intValue))
            {
                if (!intValue.HasValue)
                {
                    if (ExistingDevice == null)
                    {
                        intValue = Database.DiscoConfiguration.DeviceProfiles.DefaultAddDeviceOfflineDeviceProfileId; // Default Model for new devices
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

            var p = Cache.DeviceProfiles.FirstOrDefault(dp => dp.Id == parsedValue);

            if (p == null)
                return Error($"The identifier ({parsedValue}) does not match any Device Profile");

            friendlyValue = $"{p.Description} [{p.Id}]";

            if (ExistingDevice == null)
                return Success(EntityState.Added);
            else if (ExistingDevice != null && ExistingDevice.DeviceProfileId != parsedValue)
            {
                var previousProfile = Cache.DeviceProfiles.FirstOrDefault(dp => dp.Id == ExistingDevice.DeviceProfileId);
                friendlyPreviousValue = $"{previousProfile.Description} [{previousProfile.Id}]";

                return Success(EntityState.Modified);
            }
            else
                return Success(EntityState.Unchanged);
        }

        public override bool Apply(DiscoDataContext Database, Device Device)
        {
            if (FieldAction == EntityState.Added ||
                FieldAction == EntityState.Modified)
            {
                Device.DeviceProfileId = parsedValue;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Applied(DiscoDataContext Database, Device Device, ref bool DeviceADDescriptionSet)
        {
            if (!DeviceADDescriptionSet)
            {
                if (ActiveDirectory.IsValidDomainAccountId(Device.DeviceDomainId))
                {
                    var adAccount = Device.ActiveDirectoryAccount();

                    if (adAccount != null && !adAccount.IsCriticalSystemObject)
                    {
                        try
                        {
                            adAccount.SetDescription(Device);
                        }
                        catch (Exception ex)
                        {
                            SystemLog.LogWarning($"Unable to update AD Machine Account Description for {Device.DeviceDomainId}: {ex.Message}");
                            throw;
                        }
                        DeviceADDescriptionSet = true;
                    }
                }
            }
        }

        public override int? GuessColumn(DiscoDataContext Database, IDeviceImportContext Context, IDeviceImportDataReader DataReader)
        {
            // column name
            var possibleColumns = Context.Columns
                .Where(h => h.Type == DeviceImportFieldTypes.IgnoreColumn &&
                    h.Name.IndexOf("profile", StringComparison.OrdinalIgnoreCase) >= 0);

            // All Integers Numbers
            possibleColumns = possibleColumns.Where(h => DataReader.TestAllInt(h.Index)).ToList();

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
