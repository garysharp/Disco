using Disco.Models.Services.Devices.Importing;
using System;

namespace Disco.Services.Devices.Importing.Intune
{
    internal sealed class IntuneDeviceImportColumn : DeviceImportColumn
    {
        public Func<IntuneCsvDeviceRecord, object> ValueSelector { get; }

        private IntuneDeviceImportColumn(int index, string name, DeviceImportFieldTypes type, Type handler, Func<IntuneCsvDeviceRecord, object> valueSelector)
        {
            Index = index;
            Name = name;
            Type = type;
            Handler = handler;
            ValueSelector = valueSelector;
        }

        public static IntuneDeviceImportColumn Create<T>(int index, string name, DeviceImportFieldTypes type, Func<IntuneCsvDeviceRecord, object> valueSelector)
            where T : DeviceImportFieldBase
        {
            return new IntuneDeviceImportColumn(index, name, type, typeof(T), valueSelector);
        }
    }
}
