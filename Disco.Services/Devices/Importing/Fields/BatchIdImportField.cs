using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Disco.Services.Devices.Importing.Fields
{
    internal class BatchIdImportField : DeviceImportFieldBase
    {
        private int? parsedValue;
        private string friendlyValue;
        private string friendlyPreviousValue;

        public override DeviceImportFieldTypes FieldType { get { return DeviceImportFieldTypes.BatchId; } }

        public override object RawParsedValue { get { return parsedValue; } }
        public override string FriendlyValue { get { return friendlyValue; } }
        public override string FriendlyPreviousValue { get { return friendlyPreviousValue; } }

        public override bool Parse(DiscoDataContext Database, IDeviceImportCache Cache, IDeviceImportContext Context, string DeviceSerialNumber, Device ExistingDevice, List<IDeviceImportRecord> PreviousRecords, IDeviceImportDataReader DataReader, int ColumnIndex)
        {
            if (DataReader.TryGetNullableInt(ColumnIndex, out parsedValue))
            {
                friendlyValue = parsedValue.ToString();
            }
            else
            {
                return Error("The Batch Identifier must be a number");
            }

            if (parsedValue.HasValue)
            {
                var b = Cache.DeviceBatches.FirstOrDefault(db => db.Id == parsedValue);
                if (b == null)
                    return Error($"The identifier ({friendlyValue}) does not match any Device Batch");
                friendlyValue = $"{b.Name} [{b.Id}]";
            }
            else
                friendlyValue = null;

            if (ExistingDevice == null)
                return Success(EntityState.Added);
            else if (ExistingDevice != null && ExistingDevice.DeviceBatchId != parsedValue)
            {
                DeviceBatch previousBatch = null;
                if (ExistingDevice.DeviceBatchId.HasValue)
                    previousBatch = Cache.DeviceBatches.FirstOrDefault(db => db.Id == ExistingDevice.DeviceBatchId.Value);

                if (previousBatch != null)
                    friendlyPreviousValue = $"{previousBatch.Name} [{previousBatch.Id}]";

                return Success(EntityState.Modified);
            }
            else
                return Success(EntityState.Unchanged);
        }

        public override bool Apply(DiscoDataContext Database, Device Device)
        {
            if (FieldAction == EntityState.Added ||
                FieldAction == EntityState.Modified)
            {
                Device.DeviceBatchId = parsedValue;
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
                    h.Name.IndexOf("batch", StringComparison.OrdinalIgnoreCase) >= 0);

            // All Nullable<int> Values
            possibleColumns = possibleColumns
                .Where(h => DataReader.TestAllNullableInt(h.Index)).ToList();

            // Multiple Columns, tighten column definition
            if (possibleColumns.Count() > 1)
            {
                possibleColumns = possibleColumns
                    .Where(h =>
                        h.Name.IndexOf("batchid", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        h.Name.IndexOf("batch id", StringComparison.OrdinalIgnoreCase) >= 0);
            }

            return possibleColumns.Select(h => (int?)h.Index).FirstOrDefault();
        }
    }
}
