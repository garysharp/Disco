using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using Disco.Services.Users;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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

        public override bool Parse(DiscoDataContext Database, IDeviceImportCache Cache, DeviceImportContext Context, int RecordIndex, string DeviceSerialNumber, Device ExistingDevice, Dictionary<DeviceImportFieldTypes, string> Values, string Value)
        {
            friendlyValue = Value;

            if (string.IsNullOrWhiteSpace(Value))
            {
                friendlyValue = null;
                parsedValue = null;
            }
            else
            {
                parsedValue = Value.Trim();

                if (!parsedValue.Contains('\\'))
                    parsedValue = string.Format(@"{0}\{1}", Interop.ActiveDirectory.ActiveDirectory.Context.PrimaryDomain.NetBiosName, parsedValue);

                friendlyValue = parsedValue;

                if (parsedValue.Length > 50)
                    return Error("Cannot be more than 50 characters");
            }

            if (parsedValue != null)
            {
                // Check User Exists

                // Try Database
                User user = Database.Users.FirstOrDefault(u => u.UserId == parsedValue);
                try
                {
                    // Try Updating from AD
                    user = UserService.GetUser(parsedValue, Database);
                }
                catch (Exception ex)
                {
                    if (user == null)
                        return Error(ex.Message);
                }
                parsedValue = user.UserId;
                friendlyValue = string.Format("{0} [{1}]", user.DisplayName, user.UserId);

                // Check Decommissioned
                bool? importDecommissioning = null;
                if (Values.ContainsKey(DeviceImportFieldTypes.DeviceDecommissionedDate) || Values.ContainsKey(DeviceImportFieldTypes.DeviceDecommissionedReason))
                    importDecommissioning = Values.ContainsKey(DeviceImportFieldTypes.DeviceDecommissionedDate) && !string.IsNullOrWhiteSpace(Values[DeviceImportFieldTypes.DeviceDecommissionedDate]) ||
                        Values.ContainsKey(DeviceImportFieldTypes.DeviceDecommissionedReason) && !string.IsNullOrWhiteSpace(Values[DeviceImportFieldTypes.DeviceDecommissionedReason]);

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
                    friendlyPreviousValue = string.Format("{0} [{1}]", ExistingDevice.AssignedUser.DisplayName, ExistingDevice.AssignedUser.UserId);
                else
                    friendlyPreviousValue = null;

                return Success(EntityState.Modified);
            }
            else
                return Success(EntityState.Unchanged);
        }

        public override bool Apply(DiscoDataContext Database, Device Device)
        {
            if (this.FieldAction == EntityState.Modified)
            {
                // Remove Current Assignments
                var currentAssignments = Device.DeviceUserAssignments.Where(dua => !dua.UnassignedDate.HasValue);
                foreach (var currentAssignment in currentAssignments)
                {
                    currentAssignment.UnassignedDate = DateTime.Now;
                }

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

        public override int? GuessHeader(DiscoDataContext Database, DeviceImportContext Context)
        {
            // column name
            var possibleColumns = Context.Header
                .Select((h, i) => Tuple.Create(h, i))
                .Where(h => h.Item1.Item2 == DeviceImportFieldTypes.IgnoreColumn &&
                    h.Item1.Item1.IndexOf("user id", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Item1.Item1.IndexOf("userid", System.StringComparison.OrdinalIgnoreCase) >= 0
                    );

            return possibleColumns.Select(h => (int?)h.Item2).FirstOrDefault();
        }
    }
}
