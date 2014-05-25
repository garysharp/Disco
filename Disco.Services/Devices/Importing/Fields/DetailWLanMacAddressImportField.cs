using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Importing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Disco.Services.Devices.Importing.Fields
{
    internal class DetailWLanMacAddressImportField : DeviceImportFieldBase
    {
        private string parsedValue;
        private string previousValue;

        public override DeviceImportFieldTypes FieldType { get { return DeviceImportFieldTypes.DetailWLanMacAddress; } }

        public override object RawParsedValue { get { return parsedValue; } }
        public override string FriendlyValue { get { return parsedValue; } }
        public override string FriendlyPreviousValue { get { return previousValue; } }

        public override bool Parse(DiscoDataContext Database, IDeviceImportCache Cache, DeviceImportContext Context, int RecordIndex, string DeviceSerialNumber, Device ExistingDevice, Dictionary<DeviceImportFieldTypes, string> Values, string Value)
        {
            if (string.IsNullOrWhiteSpace(Value))
                parsedValue = null;
            else
            {
                parsedValue = Value.Trim();
            }

            if (ExistingDevice == null && parsedValue != null)
                return Success(EntityState.Added);
            else if (ExistingDevice != null)
            {
                var detail = ExistingDevice.DeviceDetails.FirstOrDefault(dd => dd.Scope == DeviceDetail.ScopeHardware && dd.Key == DeviceDetail.HardwareKeyWLanMacAddress);
                
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

        public override bool Apply(DiscoDataContext Database, Device Device)
        {
            if (this.FieldAction == EntityState.Added ||
                this.FieldAction == EntityState.Modified)
            {

                DeviceDetail detail = Database.DeviceDetails.FirstOrDefault(dd =>
                    dd.DeviceSerialNumber == Device.SerialNumber &&
                    dd.Scope == DeviceDetail.ScopeHardware &&
                    dd.Key == DeviceDetail.HardwareKeyWLanMacAddress);

                if (detail == null)
                {
                    detail = new DeviceDetail()
                    {
                        Device = Device,
                        DeviceSerialNumber = Device.SerialNumber,
                        Scope = DeviceDetail.ScopeHardware,
                        Key = DeviceDetail.HardwareKeyWLanMacAddress
                    };
                    Database.DeviceDetails.Add(detail);
                }

                detail.Value = parsedValue;
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
                .Where(h => h.Item1.Item2 == DeviceImportFieldTypes.IgnoreColumn && (
                    h.Item1.Item1.IndexOf("wireless lan mac address", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Item1.Item1.IndexOf("wireless lan address", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Item1.Item1.IndexOf("wireless mac address", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Item1.Item1.IndexOf("wireless mac", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Item1.Item1.IndexOf("wlan address", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Item1.Item1.IndexOf("wlan mac", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Item1.Item1.IndexOf("wlan mac address", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Item1.Item1.IndexOf("wlanaddress", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Item1.Item1.IndexOf("wlanmac", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Item1.Item1.IndexOf("wlanmacaddress", System.StringComparison.OrdinalIgnoreCase) >= 0
                    ));

            return possibleColumns.Select(h => (int?)h.Item2).FirstOrDefault();
        }
    }
}
