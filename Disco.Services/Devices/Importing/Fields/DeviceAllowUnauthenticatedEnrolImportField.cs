using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using Disco.Services.Users;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Disco.Services.Devices.Importing.Fields
{
    internal class DeviceAllowUnauthenticatedEnrolImportField : DeviceImportFieldBase
    {
        private static string[] TrueValues = { "true", "1", "yes", "-1", "on" };
        private static string[] FalseValues = { "false", "0", "no", "off" };
        private bool parsedValue;
        private string friendlyValue;
        private string friendlyPreviousValue;

        public override DeviceImportFieldTypes FieldType { get { return DeviceImportFieldTypes.DeviceAllowUnauthenticatedEnrol; } }

        public override object RawParsedValue { get { return parsedValue; } }
        public override string FriendlyValue { get { return friendlyValue; } }
        public override string FriendlyPreviousValue { get { return friendlyPreviousValue; } }

        public override bool Parse(DiscoDataContext Database, IDeviceImportCache Cache, DeviceImportContext Context, int RecordIndex, string DeviceSerialNumber, Device ExistingDevice, Dictionary<DeviceImportFieldTypes, string> Values, string Value)
        {
            friendlyValue = Value;

            if (!ParseBoolean(Value, out parsedValue))
                return Error("Expected a Boolean expression (True, 1, Yes, On, False, 0, No, Off)");

            friendlyValue = parsedValue.ToString();

            if (parsedValue == true)
            {
                // Check Decommissioned
                bool? importDecommissioning = null;
                if (Values.ContainsKey(DeviceImportFieldTypes.DeviceDecommissionedDate) || Values.ContainsKey(DeviceImportFieldTypes.DeviceDecommissionedReason))
                    importDecommissioning = Values.ContainsKey(DeviceImportFieldTypes.DeviceDecommissionedDate) && !string.IsNullOrWhiteSpace(Values[DeviceImportFieldTypes.DeviceDecommissionedDate]) ||
                        Values.ContainsKey(DeviceImportFieldTypes.DeviceDecommissionedReason) && !string.IsNullOrWhiteSpace(Values[DeviceImportFieldTypes.DeviceDecommissionedReason]);

                if (importDecommissioning.HasValue && importDecommissioning.Value)
                    return Error("Cannot enrol a device being decommissioned");

                if (ExistingDevice != null && ExistingDevice.DecommissionedDate.HasValue && !importDecommissioning.HasValue)
                {
                    return Error("Cannot enrol a decommissioned device");
                }
            }

            if (ExistingDevice == null && parsedValue != false) // Default: True
            {
                return Success(EntityState.Added);
            }
            else if (ExistingDevice != null && ExistingDevice.AllowUnauthenticatedEnrol != parsedValue)
            {
                friendlyPreviousValue = ExistingDevice.AllowUnauthenticatedEnrol.ToString();

                return Success(EntityState.Modified);
            }
            else
                return Success(EntityState.Unchanged);
        }

        public override bool Apply(DiscoDataContext Database, Device Device)
        {
            if (this.FieldAction == EntityState.Modified ||
                this.FieldAction == EntityState.Added)
            {
                Device.AllowUnauthenticatedEnrol = parsedValue;

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
                    h.Item1.Item1.IndexOf("trust enrol", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Item1.Item1.IndexOf("enrolment trusted", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Item1.Item1.IndexOf("trust", System.StringComparison.OrdinalIgnoreCase) >= 0
                    );

            // All Boolean
            possibleColumns = possibleColumns.Where(h =>
            {
                bool lastValue;
                return Context.RawData.Select(v => v[h.Item2]).Take(100).Where(v => !string.IsNullOrWhiteSpace(v)).All(v => ParseBoolean(v, out lastValue));
            }).ToList();

            return possibleColumns.Select(h => (int?)h.Item2).FirstOrDefault();
        }

        private static bool ParseBoolean(string value, out bool result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = false;
                return false;
            }

            value = value.Trim();

            if (TrueValues.Contains(value, StringComparer.OrdinalIgnoreCase))
            {
                result = true;
                return true;
            }
            else if (FalseValues.Contains(value, StringComparer.OrdinalIgnoreCase))
            {
                result = false;
                return true;
            }
            else
            {
                result = false;
                return false;
            }
        }
    }
}
