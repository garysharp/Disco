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

        public override bool Parse(DeviceImportDatabaseParsingState state, Device existingDevice, IDeviceImportValueReader valueReader)
        {
            if (valueReader.TryGetNullableBool(out parsedValue))
                friendlyValue = parsedValue.ToString();
            else
                return Error("Expected a Boolean expression (True, 1, Yes, On, False, 0, No, Off)");

            friendlyValue = parsedValue?.ToString() ?? "Not Set";

            if (!parsedValue.HasValue)
            {
                return Success(EntityState.Unchanged);
            }
            else if (existingDevice == null && !parsedValue.Value) // Default: True
            {
                return Success(EntityState.Added);
            }
            else if (existingDevice != null && existingDevice.AllowUnauthenticatedEnrol != parsedValue)
            {
                friendlyPreviousValue = existingDevice.AllowUnauthenticatedEnrol.ToString();

                return Success(EntityState.Modified);
            }
            else
                return Success(EntityState.Unchanged);
        }

        public override bool Parsed(DeviceImportDatabaseParsingState state, Device existingDevice, IEnumerable<IDeviceImportRecord> records, IDeviceImportRecord record)
        {
            if (parsedValue.GetValueOrDefault())
            {
                // Check Decommissioned
                var (isDecommissioning, isRecommissioning) = record.GetDecommissioningState();

                if (isDecommissioning)
                    return Error("Cannot enrol a device being decommissioned");

                if (existingDevice != null && existingDevice.DecommissionedDate.HasValue && !isRecommissioning)
                    return Error("Cannot enrol a decommissioned device");
            }

            return true;
        }

        public override bool Apply(DiscoDataContext database, Device device)
        {
            if (parsedValue.HasValue &&
                (FieldAction == EntityState.Modified ||
                FieldAction == EntityState.Added))
            {
                device.AllowUnauthenticatedEnrol = parsedValue.Value;

                return true;
            }
            else
            {
                return false;
            }
        }

        public override int? GuessColumn(IDeviceImportContext context, IDeviceImportDataReader dataReader)
        {
            // column name
            var possibleColumns = context.Columns
                .Where(h => h.Type == DeviceImportFieldTypes.IgnoreColumn &&
                    h.Name.IndexOf("trust enrol", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Name.IndexOf("enrolment trusted", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Name.IndexOf("trust", StringComparison.OrdinalIgnoreCase) >= 0
                    );

            // All Boolean
            possibleColumns = possibleColumns.Where(h => dataReader.TestAllNullableBool(h.Index)).ToList();

            return possibleColumns.Select(h => (int?)h.Index).FirstOrDefault();
        }
    }
}
