using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Users;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Disco.Services.Devices.Importing.Fields
{
    internal class AssignedUserIdImportField : DeviceImportFieldBase
    {
        private string parsedValue;
        private string friendlyValue;
        private string friendlyPreviousValue;

        public override DeviceImportFieldTypes FieldType { get { return DeviceImportFieldTypes.AssignedUserId; } }

        public override object RawParsedValue { get { return parsedValue; } }
        public override string FriendlyValue { get { return friendlyValue; } }
        public override string FriendlyPreviousValue { get { return friendlyPreviousValue; } }

        public override bool Parse(DiscoDataContext Database, IDeviceImportCache Cache, IDeviceImportContext Context, string DeviceSerialNumber, Device ExistingDevice, List<IDeviceImportRecord> PreviousRecords, IDeviceImportDataReader DataReader, int ColumnIndex)
        {
            var value = friendlyValue = DataReader.GetString(ColumnIndex);

            if (string.IsNullOrWhiteSpace(value))
            {
                friendlyValue = null;
                parsedValue = null;
            }
            else
            {
                parsedValue = value.Trim();

                parsedValue = ActiveDirectory.ParseDomainAccountId(parsedValue);

                friendlyValue = parsedValue;

                if (parsedValue.Length > 50)
                    return Error("Cannot be more than 50 characters");
            }

            if (parsedValue != null)
            {
                // Check User Exists

                // Try Database
                using (var database = new DiscoDataContext())
                {
                    User user = database.Users.FirstOrDefault(u => u.UserId == parsedValue);
                    try
                    {
                        // Try Updating from AD
                        user = UserService.GetUser(parsedValue, database);
                    }
                    catch (Exception ex)
                    {
                        if (user == null)
                            return Error(ex.Message);
                    }
                    parsedValue = user.UserId;
                    friendlyValue = $"{user.DisplayName} [{user.UserId}]";
                }

                // Check Decommissioned
                bool? importDecommissioning = null;
                int? decommissionedDateIndex = Context.GetColumnByType(DeviceImportFieldTypes.DeviceDecommissionedDate);
                int? decommissionedReasonIndex = Context.GetColumnByType(DeviceImportFieldTypes.DeviceDecommissionedReason);
                if (decommissionedDateIndex.HasValue || decommissionedReasonIndex.HasValue)
                {
                    importDecommissioning = (decommissionedDateIndex.HasValue && !string.IsNullOrWhiteSpace(DataReader.GetString(decommissionedDateIndex.Value))) ||
                        (decommissionedReasonIndex.HasValue && !string.IsNullOrWhiteSpace(DataReader.GetString(decommissionedReasonIndex.Value)));
                }

                if (importDecommissioning.HasValue && importDecommissioning.Value)
                    return Error("Cannot assign a user to a device being decommissioned");

                if (ExistingDevice != null && ExistingDevice.DecommissionedDate.HasValue && !importDecommissioning.HasValue)
                {
                    return Error("Cannot assign a user to a decommissioned device");
                }
            }

            if (ExistingDevice == null && parsedValue != null)
            {
                return Success(EntityState.Added);
            }
            else if (ExistingDevice != null && ExistingDevice.AssignedUserId != parsedValue)
            {
                if (ExistingDevice.AssignedUserId != null)
                    friendlyPreviousValue = $"{ExistingDevice.AssignedUser.DisplayName} [{ExistingDevice.AssignedUser.UserId}]";
                else
                    friendlyPreviousValue = null;

                return Success(EntityState.Modified);
            }
            else
                return Success(EntityState.Unchanged);
        }

        public override bool Apply(DiscoDataContext Database, Device Device)
        {
            if (FieldAction == EntityState.Modified)
            {
                // Remove Current Assignments
                var currentAssignments = Device.DeviceUserAssignments.Where(dua => !dua.UnassignedDate.HasValue);
                foreach (var currentAssignment in currentAssignments)
                {
                    currentAssignment.Unassign(Database);
                }
            }

            if (FieldAction == EntityState.Added || FieldAction == EntityState.Modified)
            {
                // Add Assignment
                if (parsedValue != null)
                {
                    var assignment = new DeviceUserAssignment()
                    {
                        Device = Device,
                        DeviceSerialNumber = Device.SerialNumber,
                        AssignedUserId = parsedValue,
                        AssignedDate = DateTime.Now
                    };
                    Database.DeviceUserAssignments.Add(assignment);
                }
                Device.AssignedUserId = parsedValue;

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
                        adAccount.SetDescription(Device);
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
                    h.Name.IndexOf("user", StringComparison.OrdinalIgnoreCase) >= 0);

            return possibleColumns.Select(h => (int?)h.Index).FirstOrDefault();
        }
    }
}
