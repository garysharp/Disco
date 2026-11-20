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
using System.Text.RegularExpressions;

namespace Disco.Services.Devices.Importing.Intune
{
    internal class IntuneAssignedUserIdImportField : DeviceImportFieldBase
    {
        private string userPrincipalName;
        private string userId;
        private string friendlyUserDescription;
        private string friendlyPreviousDescription;

        public override DeviceImportFieldTypes FieldType { get { return DeviceImportFieldTypes.AssignedUserId; } }

        public override object RawParsedValue { get { return userId; } }
        public override string FriendlyValue { get { return friendlyUserDescription; } }
        public override string FriendlyPreviousValue { get { return friendlyPreviousDescription; } }

        public override bool Parse(DeviceImportDatabaseParsingState state, Device existingDevice, IDeviceImportValueReader valueReader)
        {
            var upn = friendlyUserDescription = valueReader.GetString();

            if (string.IsNullOrWhiteSpace(upn))
            {
                friendlyUserDescription = null;
                userPrincipalName = null;
            }
            else
            {
                userPrincipalName = upn.Trim();

                if (!Regex.IsMatch(userPrincipalName, @"^[^@\s]+@[^@\s]+$", RegexOptions.None, TimeSpan.FromMilliseconds(100)))
                    return Error("Not a valid user principal name");

                friendlyUserDescription = userPrincipalName = upn;

                if (userPrincipalName.Length > 1024)
                    return Error("Cannot be more than 1024 characters");
            }

            if (userPrincipalName != null)
            {
                // Check User Exists

                // Try Database
                using (var database = DiscoDataContextProvider.Create())
                {
                    var user = database.Users.FirstOrDefault(u => u.UserPrincipalName == userPrincipalName);
                    try
                    {
                        if (UserService.TryGetUserByUserPrincipalName(userPrincipalName, database, false, out var adUser))
                            user = adUser;
                    }
                    catch (Exception ex)
                    {
                        if (user == null)
                            return Error(ex.Message);
                    }

                    if (user == null)
                    {
                        // try by known UserDetails
                        var userDetails = database.UserDetails.Where(d =>
                            d.Scope == "Details" &&
                            (
                                d.Key.StartsWith("Email Address") ||
                                d.Key.StartsWith("Department Email")
                            ) &&
                            d.Value == userPrincipalName)
                            .Select(d => d.User)
                            .Distinct()
                            .ToList();

                        if (userDetails.Count == 1)
                            user = userDetails[0];
                        else if (userDetails.Count > 1)
                            return Error($"Multiple user details found with this email address: {string.Join(", ", userDetails.Take(3).Select(u => u.UserId))}");
                    }

                    if (user == null)
                        return Error("User not found");
                    userId = user.UserId;
                    userPrincipalName = user.UserPrincipalName;
                    friendlyUserDescription = $"{user.DisplayName} [{user.UserId}]";
                }
            }

            if (existingDevice == null && userPrincipalName != null)
            {
                return Success(EntityState.Added);
            }
            else if (existingDevice != null && existingDevice.AssignedUserId != userId)
            {
                if (existingDevice.AssignedUserId != null)
                    friendlyPreviousDescription = $"{existingDevice.AssignedUser.DisplayName} [{existingDevice.AssignedUser.UserId}]";
                else
                    friendlyPreviousDescription = null;

                return Success(EntityState.Modified);
            }
            else
                return Success(EntityState.Unchanged);
        }

        public override bool Parsed(DeviceImportDatabaseParsingState state, Device existingDevice, IEnumerable<IDeviceImportRecord> records, IDeviceImportRecord record)
        {
            if (userPrincipalName != null)
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
                if (userId != null)
                {
                    var user = database.Users.FirstOrDefault(u => u.UserId == userId);
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
            => throw new NotImplementedException();
    }
}
