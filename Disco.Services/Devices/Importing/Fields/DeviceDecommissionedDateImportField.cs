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

        public override bool Parse(DiscoDataContext Database, IDeviceImportCache Cache, IDeviceImportContext Context, string DeviceSerialNumber, Device ExistingDevice, List<IDeviceImportRecord> PreviousRecords, IDeviceImportDataReader DataReader, int ColumnIndex)
        {
            if (!DataReader.TryGetNullableDateTime(ColumnIndex, out parsedValue))
            {
                rawValue = DataReader.GetString(ColumnIndex);
                return Error($"Cannot parse the value as a Date/Time using {CultureInfo.CurrentCulture.Name} culture (system default).");
            }

            if (parsedValue.HasValue)
            {
                // Accuracy to the second (remove any milliseconds)
                parsedValue = new DateTime((parsedValue.Value.Ticks / 10000000L) * 10000000L);
            }

            string errorMessage;
            if (parsedValue.HasValue && !CanDecommissionDevice(ExistingDevice, Context, DataReader, out errorMessage))
                return Error(errorMessage);

            var decommissionReasonIndex = Context.GetColumnByType(DeviceImportFieldTypes.DeviceDecommissionedReason);
            setReason = !decommissionReasonIndex.HasValue ||
                (parsedValue.HasValue && string.IsNullOrWhiteSpace(DataReader.GetString(decommissionReasonIndex.Value)));

            if (ExistingDevice != null && ExistingDevice.DecommissionedDate.HasValue)
            {
                // Accuracy to the second (remove any milliseconds)
                previousValue = new DateTime((ExistingDevice.DecommissionedDate.Value.Ticks / 10000000L) * 10000000L);
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

        public override bool Apply(DiscoDataContext Database, Device Device)
        {
            if (FieldAction == EntityState.Modified)
            {
                // Decommission or Recommission Device
                Device.DecommissionedDate = parsedValue;

                if (setReason)
                {
                    if (parsedValue.HasValue && !Device.DecommissionReason.HasValue)
                    {
                        Device.DecommissionReason = DecommissionReasons.EndOfLife;
                    }
                    else if (!parsedValue.HasValue && Device.DecommissionReason.HasValue)
                    {
                        Device.DecommissionReason = null;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Applied(DiscoDataContext Database, Device Device, ref bool DeviceADDescriptionSet)
        {
            if (ActiveDirectory.IsValidDomainAccountId(Device.DeviceDomainId))
            {
                var adAccount = Device.ActiveDirectoryAccount();

                if (adAccount != null && !adAccount.IsCriticalSystemObject)
                {
                    if (Device.DecommissionedDate.HasValue)
                    {
                        // Disable AD Account
                        adAccount.DisableAccount();
                    }
                    else
                    {
                        // Enable AD Account
                        adAccount.EnableAccount();
                    }

                    if (!DeviceADDescriptionSet)
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
                    h.Name.IndexOf("decommission date", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Name.IndexOf("decommissiondate", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Name.IndexOf("decommissioned date", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Name.IndexOf("decommissioneddate", StringComparison.OrdinalIgnoreCase) >= 0
                    );

            possibleColumns = possibleColumns
                .Where(h => DataReader.TestAllNullableDateTime(h.Index)).ToList();

            return possibleColumns.Select(h => (int?)h.Index).FirstOrDefault();
        }

        public static bool CanDecommissionDevice(Device Device, IDeviceImportContext Context, IDeviceImportDataReader DataReader, out string ErrorMessage)
        {
            if (Device == null)
            {
                ErrorMessage = "Cannot decommission new devices";
                return false;
            }

            // Check device is assigned (or being removed in this import)

            var assignedUserIndex = Context.GetColumnByType(DeviceImportFieldTypes.AssignedUserId);
            if ((!assignedUserIndex.HasValue && Device.AssignedUserId != null) ||
                (assignedUserIndex.HasValue && !string.IsNullOrWhiteSpace(DataReader.GetString(assignedUserIndex.Value))))
            {
                if (Device.AssignedUserId != null)
                    ErrorMessage = $"The device is assigned to a user ({Device.AssignedUser.DisplayName} [{Device.AssignedUser.UserId}]) and cannot be decommissioned";
                else
                    ErrorMessage = $"The device is being assigned to a user ({DataReader.GetString(assignedUserIndex.Value)}) and cannot be decommissioned";
                return false;
            }

            // Check device doesn't have any open jobs
            var openJobCount = Device.Jobs.Count(j => !j.ClosedDate.HasValue);
            if (openJobCount > 0)
            {
                ErrorMessage = $"The device is associated with {openJobCount} open job{(openJobCount == 1 ? null : "s")} and cannot be decommissioned";
                return false;
            }

            ErrorMessage = null;
            return true;
        }
    }
}
