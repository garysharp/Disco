using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Logging;
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

        public static AssignedUserIdImportField CreateUnassigned(Device device)
        {
            var field = new AssignedUserIdImportField()
            {
                parsedValue = null,
                friendlyValue = null,
            };
            if (device.AssignedUser != null)
            {
                field.friendlyPreviousValue = $"{device.AssignedUser.DisplayName} [{device.AssignedUser.UserId}]";
                field.Success(EntityState.Modified);
            }
            else
            {
                field.Success(EntityState.Unchanged);
            }
            return field;
        }

        public override bool Parse(DeviceImportDatabaseParsingState state, Device existingDevice, IDeviceImportValueReader valueReader)
        {
            var value = friendlyValue = valueReader.GetString();

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
                    var user = database.Users.FirstOrDefault(u => u.UserId == parsedValue);
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
            }

            if (existingDevice == null && parsedValue != null)
            {
                return Success(EntityState.Added);
            }
            else if (existingDevice != null && existingDevice.AssignedUserId != parsedValue)
            {
                if (existingDevice.AssignedUserId != null)
                    friendlyPreviousValue = $"{existingDevice.AssignedUser.DisplayName} [{existingDevice.AssignedUser.UserId}]";
                else
                    friendlyPreviousValue = null;

                return Success(EntityState.Modified);
            }
            else
                return Success(EntityState.Unchanged);
        }

        public override bool Parsed(DeviceImportDatabaseParsingState state, Device existingDevice, IEnumerable<IDeviceImportRecord> records, IDeviceImportRecord record)
        {
            if (parsedValue != null)
            {
                // Check Decommissioned
                var (isDecommissioning, isRecommissioning) = record.GetDecommissioningState();

                if (isDecommissioning)
                    return Error("Cannot assign a user to a device being decommissioned");

                if (existingDevice != null && existingDevice.DecommissionedDate.HasValue && !isRecommissioning)
                    return Error("Cannot assign a user to a decommissioned device");
            }

            return true;
        }

        public override bool Apply(DiscoDataContext database, Device device)
        {
            if (FieldAction == EntityState.Modified)
            {
                // Remove Current Assignments
                var currentAssignments = device.DeviceUserAssignments.Where(dua => !dua.UnassignedDate.HasValue);
                foreach (var currentAssignment in currentAssignments)
                {
                    currentAssignment.Unassign(database);
                }
            }

            if (FieldAction == EntityState.Added || FieldAction == EntityState.Modified)
            {
                // Add Assignment
                if (parsedValue != null)
                {
                    var user = database.Users.FirstOrDefault(u => u.UserId == parsedValue);
                    var assignment = new DeviceUserAssignment()
                    {
                        Device = device,
                        DeviceSerialNumber = device.SerialNumber,
                        AssignedUserId = user.UserId,
                        AssignedUser = user,
                        AssignedDate = DateTime.Now
                    };
                    database.DeviceUserAssignments.Add(assignment);
                    device.AssignedUser = user;
                    device.AssignedUserId = user.UserId;
                }
                else
                {
                    device.AssignedUserId = null;
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
            if (!deviceADDescriptionSet &&
                ActiveDirectory.IsValidDomainAccountId(device.DeviceDomainId))
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
                    h.Name.IndexOf("user", StringComparison.OrdinalIgnoreCase) >= 0);

            return possibleColumns.Select(h => (int?)h.Index).FirstOrDefault();
        }
    }
}
