using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Logging;
using System;
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

        public override bool Parse(DeviceImportDatabaseParsingState state, Device existingDevice, IDeviceImportValueReader valueReader)
        {
            if (valueReader.TryGetNullableInt(out var intValue))
            {
                if (!intValue.HasValue)
                {
                    if (existingDevice == null)
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

            var m = state.DeviceModels.FirstOrDefault(dm => dm.Id == parsedValue);

            if (m == null)
                return Error($"The identifier ({parsedValue}) does not match any Device Model");

            friendlyValue = $"{m.Description} [{m.Id}]";

            if (existingDevice == null)
                return Success(EntityState.Added);
            else if (existingDevice != null && existingDevice.DeviceModelId != parsedValue)
            {
                friendlyPreviousValue = null;
                if (existingDevice.DeviceModelId.HasValue)
                {
                    var previousModel = state.DeviceModels.FirstOrDefault(dm => dm.Id == existingDevice.DeviceModelId.Value);
                    friendlyPreviousValue = $"{previousModel.Description} [{previousModel.Id}]";
                }

                return Success(EntityState.Modified);
            }
            else
                return Success(EntityState.Unchanged);
        }

        public override bool Apply(DiscoDataContext database, Device device)
        {
            if (FieldAction == EntityState.Added ||
                FieldAction == EntityState.Modified)
            {
                device.DeviceModelId = parsedValue;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Applied(DiscoDataContext database, Device device, ref bool deviceADDescriptionSet)
        {
            if (!deviceADDescriptionSet && ActiveDirectory.IsValidDomainAccountId(device.DeviceDomainId))
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
                    h.Name.IndexOf("model", StringComparison.OrdinalIgnoreCase) >= 0);

            // All Integers Numbers
            possibleColumns = possibleColumns.Where(h => dataReader.TestAllInt(h.Index)).ToList();

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
