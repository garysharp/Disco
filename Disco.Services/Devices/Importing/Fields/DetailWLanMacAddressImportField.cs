﻿using Disco.Data.Repository;
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

        public override bool Parse(DiscoDataContext Database, IDeviceImportCache Cache, IDeviceImportContext Context, string DeviceSerialNumber, Device ExistingDevice, List<IDeviceImportRecord> PreviousRecords, IDeviceImportDataReader DataReader, int ColumnIndex)
        {
            var value = DataReader.GetString(ColumnIndex);

            if (string.IsNullOrWhiteSpace(value))
                parsedValue = null;
            else
            {
                parsedValue = value.Trim();
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
            if (FieldAction == EntityState.Added ||
                FieldAction == EntityState.Modified)
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

        public override int? GuessColumn(DiscoDataContext Database, IDeviceImportContext Context, IDeviceImportDataReader DataReader)
        {
            // column name
            var possibleColumns = Context.Columns
                .Where(h => h.Type == DeviceImportFieldTypes.IgnoreColumn && (
                    h.Name.IndexOf("wireless lan mac address", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Name.IndexOf("wireless lan address", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Name.IndexOf("wireless mac address", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Name.IndexOf("wireless mac", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Name.IndexOf("wlan address", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Name.IndexOf("wlan mac", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Name.IndexOf("wlan mac address", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Name.IndexOf("wlanaddress", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Name.IndexOf("wlanmac", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    h.Name.IndexOf("wlanmacaddress", StringComparison.OrdinalIgnoreCase) >= 0
                    ));

            return possibleColumns.Select(h => (int?)h.Index).FirstOrDefault();
        }
    }
}
