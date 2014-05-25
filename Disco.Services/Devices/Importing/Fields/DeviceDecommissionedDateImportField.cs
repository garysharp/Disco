using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
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

        public override bool Parse(DiscoDataContext Database, IDeviceImportCache Cache, DeviceImportContext Context, int RecordIndex, string DeviceSerialNumber, Device ExistingDevice, Dictionary<DeviceImportFieldTypes, string> Values, string Value)
        {
            if (string.IsNullOrWhiteSpace(Value))
            {
                rawValue = null;
                parsedValue = null;
            }
            else
            {
                DateTime valueDateTime;
                if (!DateTime.TryParse(Value.Trim(), CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.AssumeLocal, out valueDateTime))
                {
                    rawValue = Value.Trim();
                    return Error(string.Format("Cannot parse the value as a Date/Time using {0} culture (system default).", CultureInfo.CurrentCulture.Name));
                }
                else
                {
                    // Accuracy to the second (remove any milliseconds)
                    parsedValue = new DateTime((valueDateTime.Ticks / 10000000L) * 10000000L);
                }
            }

            string errorMessage;
            if (parsedValue.HasValue && !CanDecommissionDevice(ExistingDevice, Values, out errorMessage))
                return Error(errorMessage);

            setReason = !Values.ContainsKey(DeviceImportFieldTypes.DeviceDecommissionedReason) ||
                (parsedValue.HasValue && string.IsNullOrWhiteSpace(Values[DeviceImportFieldTypes.DeviceDecommissionedReason]));

            if (ExistingDevice != null && ExistingDevice.DecommissionedDate != parsedValue)
            {
                previousValue = ExistingDevice.DecommissionedDate;
                return Success(EntityState.Modified);
            }
            else
                return Success(EntityState.Unchanged);
        }

        public override bool Apply(DiscoDataContext Database, Device Device)
        {
            if (this.FieldAction == EntityState.Modified)
            {
                // Decommission or Recommission Device
                Device.DecommissionedDate = this.parsedValue;

                if (setReason)
                    Device.DecommissionReason = this.parsedValue.HasValue ? (DecommissionReasons?)DecommissionReasons.EndOfLife : null;

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
                    h.Item1.Item1.IndexOf("decommission date", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Item1.Item1.IndexOf("decommissiondate", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Item1.Item1.IndexOf("decommissioned date", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Item1.Item1.IndexOf("decommissioneddate", System.StringComparison.OrdinalIgnoreCase) >= 0
                    );

            return possibleColumns.Select(h => (int?)h.Item2).FirstOrDefault();
        }

        public static bool CanDecommissionDevice(Device Device, Dictionary<DeviceImportFieldTypes, string> Values, out string ErrorMessage)
        {
            if (Device == null)
            {
                ErrorMessage = "Cannot decommission new devices";
                return false;
            }

            // Check device is assigned (or being removed in this import)

            if ((!Values.ContainsKey(DeviceImportFieldTypes.AssignedUserId) && Device.AssignedUserId != null) ||
                (Values.ContainsKey(DeviceImportFieldTypes.AssignedUserId) && !string.IsNullOrWhiteSpace(Values[DeviceImportFieldTypes.AssignedUserId])))
            {
                if (Device.AssignedUserId != null)
                    ErrorMessage = string.Format("The device is assigned to a user ({0} [{1}]) and cannot be decommissioned", Device.AssignedUser.DisplayName, Device.AssignedUser.UserId);
                else
                    ErrorMessage = string.Format("The device is being assigned to a user ({0}) and cannot be decommissioned", Values[DeviceImportFieldTypes.AssignedUserId]);
                return false;
            }

            // Check device doesn't have any open jobs
            var openJobCount = Device.Jobs.Count(j => !j.ClosedDate.HasValue);
            if (openJobCount > 0)
            {
                ErrorMessage = string.Format("The device is associated with {0} open job{1} and cannot be decommissioned", openJobCount, openJobCount == 1 ? null : "s");
                return false;
            }

            ErrorMessage = null;
            return true;
        }
    }
}
