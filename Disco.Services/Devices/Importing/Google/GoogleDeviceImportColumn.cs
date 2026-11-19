using Disco.Models.Services.Devices.Importing;
using System;

namespace Disco.Services.Devices.Importing.Google
{
    internal sealed class GoogleDeviceImportColumn : DeviceImportColumn
    {
        public Func<GoogleCsvDeviceRecord, object> ValueSelector { get; }

        private GoogleDeviceImportColumn(int index, string name, DeviceImportFieldTypes type, Type handler, Func<GoogleCsvDeviceRecord, object> valueSelector)
        {
            Index = index;
            Name = name;
            Type = type;
            Handler = handler;
            ValueSelector = valueSelector;
        }

        public static GoogleDeviceImportColumn Create<T>(int index, string name, DeviceImportFieldTypes type, Func<GoogleCsvDeviceRecord, object> valueSelector)
            where T : DeviceImportFieldBase
        {
            return new GoogleDeviceImportColumn(index, name, type, typeof(T), valueSelector);
        }
    }
}
