using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using Disco.Services.Interop.ActiveDirectory;
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

        public override bool Parse(DiscoDataContext Database, IDeviceImportCache Cache, IDeviceImportContext Context, string DeviceSerialNumber, Device ExistingDevice, List<IDeviceImportRecord> PreviousRecords, IDeviceImportDataReader DataReader, int ColumnIndex)
        {
            int? intValue;
            if (DataReader.TryGetNullableInt(ColumnIndex, out intValue))
            {
                if (!intValue.HasValue)
                {
                    if (ExistingDevice == null)
                    {
                        intValue = 1; // Default Model for new devices
                    }
                    else
                    {
                        return Error("The Model Identifier cannot be blank");
                    }
                }

                parsedValue = intValue.Value;
                friendlyValue = parsedValue.ToString();
            }
            else
            {
                return Error("The Model Identifier must be a number");
            }

            var m = Cache.DeviceModels.FirstOrDefault(dm => dm.Id == parsedValue);

            if (m == null)
                return Error($"The identifier ({parsedValue}) does not match any Device Model");

            friendlyValue = $"{m.Description} [{m.Id}]";

            if (ExistingDevice == null)
                return Success(EntityState.Added);
            else if (ExistingDevice != null && ExistingDevice.DeviceModelId != parsedValue)
            {
                friendlyPreviousValue = null;
                if (ExistingDevice.DeviceModelId.HasValue)
                {
                    var previousModel = Cache.DeviceModels.FirstOrDefault(dm => dm.Id == ExistingDevice.DeviceModelId.Value);
                    friendlyPreviousValue = $"{previousModel.Description} [{previousModel.Id}]";
                }

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
                Device.DeviceModelId = parsedValue;
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
                    h.Name.IndexOf("model", StringComparison.OrdinalIgnoreCase) >= 0);

            // All Integers Numbers
            possibleColumns = possibleColumns.Where(h => DataReader.TestAllInt(h.Index)).ToList();

            // Multiple Columns, tighten column definition
            if (possibleColumns.Count() > 1)
            {
                possibleColumns = possibleColumns
                    .Where(h =>
                        h.Name.IndexOf("modelid", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        h.Name.IndexOf("model id", StringComparison.OrdinalIgnoreCase) >= 0);
            }

            return possibleColumns.Select(h => (int?)h.Index).FirstOrDefault();
        }
    }
}
