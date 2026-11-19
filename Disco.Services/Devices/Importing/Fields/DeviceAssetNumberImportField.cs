using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using System;
using System.Data;
using System.Linq;

namespace Disco.Services.Devices.Importing.Fields
{
    internal class DeviceAssetNumberImportField : DeviceImportFieldBase
    {
        private string parsedValue;
        private string previousValue;

        public override DeviceImportFieldTypes FieldType { get { return DeviceImportFieldTypes.DeviceAssetNumber; } }

        public override object RawParsedValue { get { return parsedValue; } }
        public override string FriendlyValue { get { return parsedValue; } }
        public override string FriendlyPreviousValue { get { return previousValue; } }

        public override bool Parse(DeviceImportDatabaseParsingState state, Device existingDevice, IDeviceImportValueReader valueReader)
        {
            var value = valueReader.GetString();

            if (string.IsNullOrWhiteSpace(value))
                parsedValue = null;
            else
            {
                parsedValue = value.Trim();
                if (parsedValue.Length > 40)
                    return Error("Cannot be more than 40 characters");
            }

            if (existingDevice == null && parsedValue != null)
                return Success(EntityState.Added);
            else if (existingDevice != null && existingDevice.AssetNumber != parsedValue)
            {
                previousValue = existingDevice.AssetNumber;
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
                device.AssetNumber = parsedValue;
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
                    h.Name.IndexOf("asset", StringComparison.OrdinalIgnoreCase) >= 0);

            return possibleColumns.Select(h => (int?)h.Index).FirstOrDefault();
        }
    }
}
