using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Disco.Services.Devices.Importing.Fields
{
    internal class DeviceAllowUnauthenticatedEnrolImportField : DeviceImportFieldBase
    {
        private bool? parsedValue;
        private string friendlyValue;
        private string friendlyPreviousValue;

        public override DeviceImportFieldTypes FieldType { get { return DeviceImportFieldTypes.DeviceAllowUnauthenticatedEnrol; } }

        public override object RawParsedValue { get { return parsedValue; } }
        public override string FriendlyValue { get { return friendlyValue; } }
        public override string FriendlyPreviousValue { get { return friendlyPreviousValue; } }

        public override bool Parse(DiscoDataContext Database, IDeviceImportCache Cache, IDeviceImportContext Context, string DeviceSerialNumber, Device ExistingDevice, List<IDeviceImportRecord> PreviousRecords, IDeviceImportDataReader DataReader, int ColumnIndex)
        {
            if (DataReader.TryGetNullableBool(ColumnIndex, out parsedValue))
            {
                friendlyValue = parsedValue.ToString();
            }
            else
            {
                return Error("Expected a Boolean expression (True, 1, Yes, On, False, 0, No, Off)");
            }

            friendlyValue = parsedValue?.ToString() ?? "Not Set";

            if (parsedValue.HasValue && parsedValue.Value == true)
            {
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
                    return Error("Cannot enrol a device being decommissioned");

                if (ExistingDevice != null && ExistingDevice.DecommissionedDate.HasValue && !importDecommissioning.HasValue)
                {
                    return Error("Cannot enrol a decommissioned device");
                }
            }

            if (!parsedValue.HasValue)
            {
                return Success(EntityState.Unchanged);
            }
            else if (ExistingDevice == null && parsedValue.Value != false) // Default: True
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
            if (parsedValue.HasValue &&
                (FieldAction == EntityState.Modified ||
                FieldAction == EntityState.Added))
            {
                Device.AllowUnauthenticatedEnrol = parsedValue.Value;

                return true;
            }
            else
            {
                return false;
            }
        }

        public override int? GuessColumn(DiscoDataContext Database, IDeviceImportContext Context, IDeviceImportDataReader DataReader)
        {
            // column name
            var possibleColumns = Context.Columns
                .Where(h => h.Type == DeviceImportFieldTypes.IgnoreColumn &&
                    h.Name.IndexOf("trust enrol", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Name.IndexOf("enrolment trusted", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Name.IndexOf("trust", StringComparison.OrdinalIgnoreCase) >= 0
                    );

            // All Boolean
            possibleColumns = possibleColumns.Where(h => DataReader.TestAllNullableBool(h.Index)).ToList();

            return possibleColumns.Select(h => (int?)h.Index).FirstOrDefault();
        }
    }
}
