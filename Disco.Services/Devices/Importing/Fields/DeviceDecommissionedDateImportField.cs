using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace Disco.Services.Devices.Importing.Fields
{
    internal class DeviceDecommissionedDateImportField : DeviceImportFieldBase
    {
        private const string DateFormat = "yyyy-MM-dd HH:mm:ss";

        private string rawValue;
        private DateTime? parsedValue;
        private DateTime? previousValue;
        private bool setReason { get; set; }

        public override DeviceImportFieldTypes FieldType { get { return DeviceImportFieldTypes.DeviceDecommissionedDate; } }

        public override object RawParsedValue { get { return parsedValue; } }
        public override string FriendlyValue { get { return parsedValue.HasValue ? parsedValue.Value.ToString(DateFormat) : rawValue; } }
        public override string FriendlyPreviousValue { get { return previousValue.HasValue ? previousValue.Value.ToString(DateFormat) : null; } }

        public override bool Parse(DeviceImportDatabaseParsingState state, Device existingDevice, IDeviceImportValueReader valueReader)
        {
            if (!valueReader.TryGetNullableDateTime(out parsedValue))
            {
                rawValue = valueReader.GetString();
                return Error($"Cannot parse the value as a Date/Time using {CultureInfo.CurrentCulture.Name} culture (system default).");
            }

            if (parsedValue.HasValue)
            {
                // Accuracy to the second (remove any milliseconds)
                parsedValue = new DateTime((parsedValue.Value.Ticks / 10000000L) * 10000000L, DateTimeKind.Unspecified);
            }

            if (existingDevice != null && existingDevice.DecommissionedDate.HasValue)
            {
                // Accuracy to the second (remove any milliseconds)
                previousValue = new DateTime((existingDevice.DecommissionedDate.Value.Ticks / 10000000L) * 10000000L, DateTimeKind.Unspecified);
            }
            else
            {
                previousValue = null;
            }

            if (previousValue != parsedValue)
            {
                return Success(EntityState.Modified);
            }
            else
            {
                previousValue = null;
                return Success(EntityState.Unchanged);
            }
        }

        public override bool Parsed(DeviceImportDatabaseParsingState state, Device existingDevice, IEnumerable<IDeviceImportRecord> records, IDeviceImportRecord record)
        {
            if (parsedValue.HasValue && !CanDecommissionDevice(existingDevice, record, out var errorMessage))
                return Error(errorMessage);

            var decommissionReasonField = record.Fields.FirstOrDefault(f => f.FieldType == DeviceImportFieldTypes.DeviceDecommissionedReason);
            setReason = decommissionReasonField == null ||
                (parsedValue.HasValue && decommissionReasonField.RawParsedValue == null);

            return true;
        }

        public override bool Apply(DiscoDataContext database, Device device)
        {
            if (FieldAction == EntityState.Modified)
            {
                // Decommission or Recommission Device
                device.DecommissionedDate = parsedValue;

                if (setReason)
                {
                    if (parsedValue.HasValue && !device.DecommissionReason.HasValue)
                    {
                        device.DecommissionReason = DecommissionReasons.EndOfLife;
                    }
                    else if (!parsedValue.HasValue && device.DecommissionReason.HasValue)
                    {
                        device.DecommissionReason = null;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Applied(DiscoDataContext database, Device device, ref bool deviceADDescriptionSet)
        {
            if (ActiveDirectory.IsValidDomainAccountId(device.DeviceDomainId))
            {
                // Don't disable if another active device with the same name exists
                var duplicateNamedDevice = database.Devices
                    .Any(i => i.DeviceDomainId == device.DeviceDomainId &&
                                i.SerialNumber != device.SerialNumber &&
                                i.DecommissionedDate == null);
                if (!duplicateNamedDevice)
                {
                    var adAccount = device.ActiveDirectoryAccount();

                    if (adAccount != null && !adAccount.IsCriticalSystemObject)
                    {
                        if (device.DecommissionedDate.HasValue)
                        {
                            // Disable AD Account
                            adAccount.DisableAccount();
                        }
                        else
                        {
                            // Enable AD Account
                            adAccount.EnableAccount();
                        }

                        if (!deviceADDescriptionSet)
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
            }
        }

        public override int? GuessColumn(IDeviceImportContext context, IDeviceImportDataReader dataReader)
        {
            // column name
            var possibleColumns = context.Columns
                .Where(h => h.Type == DeviceImportFieldTypes.IgnoreColumn &&
                    h.Name.IndexOf("decommission date", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Name.IndexOf("decommissiondate", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Name.IndexOf("decommissioned date", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Name.IndexOf("decommissioneddate", StringComparison.OrdinalIgnoreCase) >= 0
                    );

            possibleColumns = possibleColumns
                .Where(h => dataReader.TestAllNullableDateTime(h.Index)).ToList();

            return possibleColumns.Select(h => (int?)h.Index).FirstOrDefault();
        }

        public static bool CanDecommissionDevice(Device device, IDeviceImportRecord record, out string errorMessage)
        {
            var isAssigningUser = false;
            var assigningUserId = default(string);
            var assignedUserField = record.Fields.FirstOrDefault(f => f.FieldType == DeviceImportFieldTypes.AssignedUserId);
            if (assignedUserField != null)
            {
                isAssigningUser = true;
                assigningUserId = (string)assignedUserField.RawParsedValue;
            }
            var hasOpenJobs = device.Jobs.Any(j => !j.ClosedDate.HasValue);

            return CanDecommissionDevice(device, isAssigningUser, assigningUserId, hasOpenJobs, out errorMessage);
        }

        public static bool CanDecommissionDevice(Device device, bool isAssigningUser, string assigningUserId, bool hasOpenJobs, out string errorMessage)
        {
            if (device == null)
            {
                errorMessage = "Cannot decommission new devices";
                return false;
            }

            // Check device is assigned (or being removed in this import)
            if (isAssigningUser && !string.IsNullOrEmpty(assigningUserId) ||
                (!isAssigningUser && device.AssignedUserId != null))
            {
                if (!isAssigningUser)
                    errorMessage = $"The device is assigned to a user ({device.AssignedUser.DisplayName} [{device.AssignedUser.UserId}]) and cannot be decommissioned";
                else
                    errorMessage = $"The device is being assigned to a user ({assigningUserId}) and cannot be decommissioned";
                return false;
            }

            // Check device doesn't have any open jobs
            if (hasOpenJobs)
            {
                errorMessage = $"The device is associated with an open job and cannot be decommissioned";
                return false;
            }

            errorMessage = null;
            return true;
        }
    }
}
