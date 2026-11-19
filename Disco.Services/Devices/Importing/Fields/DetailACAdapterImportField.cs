using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using System;
using System.Data;
using System.Linq;

namespace Disco.Services.Devices.Importing.Fields
{
    internal class DetailACAdapterImportField : DeviceImportFieldBase
    {
        private string parsedValue;
        private string previousValue;

        public override DeviceImportFieldTypes FieldType { get { return DeviceImportFieldTypes.DetailACAdapter; } }

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
            }

            if (existingDevice == null && parsedValue != null)
                return Success(EntityState.Added);
            else if (existingDevice != null)
            {
                var detail = existingDevice.DeviceDetails.FirstOrDefault(dd => dd.Scope == DeviceDetail.ScopeHardware && dd.Key == DeviceDetail.HardwareKeyACAdapter);

                if (detail == null && parsedValue == null)
                    return Success(EntityState.Unchanged);
                else if (detail == null && parsedValue != null)
                {
                    return Success(EntityState.Modified);
                }
                else if (detail.Value != parsedValue)
                {
                    previousValue = detail.Value;
                    return Success(EntityState.Modified);
                }
                else
                    return Success(EntityState.Unchanged);
            }
            else
                return Success(EntityState.Unchanged);
        }

        public override bool Apply(DiscoDataContext database, Device device)
        {
            if (FieldAction == EntityState.Added ||
                FieldAction == EntityState.Modified)
            {

                DeviceDetail detail = database.DeviceDetails.FirstOrDefault(dd =>
                    dd.DeviceSerialNumber == device.SerialNumber &&
                    dd.Scope == DeviceDetail.ScopeHardware &&
                    dd.Key == DeviceDetail.HardwareKeyACAdapter);

                if (detail == null)
                {
                    detail = new DeviceDetail()
                    {
                        Device = device,
                        DeviceSerialNumber = device.SerialNumber,
                        Scope = DeviceDetail.ScopeHardware,
                        Key = DeviceDetail.HardwareKeyACAdapter
                    };
                    database.DeviceDetails.Add(detail);
                }

                detail.Value = parsedValue;
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
                    (h.Name.IndexOf("ac adapter", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Name.IndexOf("acadapter", StringComparison.OrdinalIgnoreCase) >= 0));

            return possibleColumns.Select(h => (int?)h.Index).FirstOrDefault();
        }
    }
}
