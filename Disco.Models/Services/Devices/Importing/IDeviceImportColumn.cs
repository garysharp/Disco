using System;

namespace Disco.Models.Services.Devices.Importing
{
    public interface IDeviceImportColumn
    {
        int Index { get; }
        string Name { get; }
        DeviceImportFieldTypes Type { get; }
        Type Handler { get; }

        IDeviceImportField GetHandlerInstance();
    }
}
