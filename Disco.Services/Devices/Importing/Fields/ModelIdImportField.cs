using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Disco.Services.Devices.Importing.Fields
{
    internal class ModelIdImportField : DeviceImportFieldBase
    {
        private int parsedValue;
        private string friendlyValue;
        private string friendlyPreviousValue;

        public override DeviceImportFieldTypes FieldType { get { return DeviceImportFieldTypes.ModelId; } }

        public override object RawParsedValue { get { return parsedValue; } }
        public override string FriendlyValue { get { return friendlyValue; } }
        public override string FriendlyPreviousValue { get { return friendlyPreviousValue; } }

        public override bool Parse(DiscoDataContext Database, IDeviceImportCache Cache, DeviceImportContext Context, int RecordIndex, string DeviceSerialNumber, Device ExistingDevice, Dictionary<DeviceImportFieldTypes, string> Values, string Value)
        {
            friendlyValue = Value;

            // Validate
            if (string.IsNullOrWhiteSpace(Value))
                this.parsedValue = 1; // Default Model
            else
                if (!int.TryParse(Value, out parsedValue))
                    return Error("The Model Identifier must be a number");

            var m = Cache.DeviceModels.FirstOrDefault(dm => dm.Id == parsedValue);

            if (m == null)
                return Error(string.Format("The identifier ({0}) does not match any Device Model", Value));

            friendlyValue = string.Format("{0} [{1}]", m.Description, m.Id);

            if (ExistingDevice == null)
                return Success(EntityState.Added);
            else if (ExistingDevice != null && ExistingDevice.DeviceModelId != parsedValue)
            {
                friendlyPreviousValue = null;
                if (ExistingDevice.DeviceModelId.HasValue)
                {
                    var previousModel = Cache.DeviceModels.FirstOrDefault(dm => dm.Id == ExistingDevice.DeviceModelId.Value);
                    friendlyPreviousValue = string.Format("{0} [{1}]", previousModel.Description, previousModel.Id);
                }

                return Success(EntityState.Modified);
            }
            else
                return Success(EntityState.Unchanged);
        }

        public override bool Apply(DiscoDataContext Database, Device Device)
        {
            if (this.FieldAction == EntityState.Added ||
                this.FieldAction == EntityState.Modified)
            {
                Device.DeviceModelId = this.parsedValue;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override int? GuessHeader(DiscoDataContext Database, DeviceImportContext Context)
        {
            // 'model' in column name
            var possibleColumns = Context.Header
                .Select((h, i) => Tuple.Create(h, i))
                .Where(h => h.Item1.Item2 == DeviceImportFieldTypes.IgnoreColumn && h.Item1.Item1.IndexOf("model", System.StringComparison.OrdinalIgnoreCase) >= 0);

            // All Integers Numbers
            possibleColumns = possibleColumns.Where(h =>
            {
                int lastValue;
                return Context.RawData.Select(v => v[h.Item2]).Take(100).Where(v => !string.IsNullOrWhiteSpace(v)).All(v => int.TryParse(v, out lastValue));
            }).ToList();

            // Multiple Columns, tighten column definition
            if (possibleColumns.Count() > 1)
            {
                possibleColumns = possibleColumns
                    .Where(h =>
                        h.Item1.Item1.IndexOf("modelid", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        h.Item1.Item1.IndexOf("model id", StringComparison.OrdinalIgnoreCase) >= 0);
            }

            return possibleColumns.Select(h => (int?)h.Item2).FirstOrDefault();
        }
    }
}
