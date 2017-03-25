using Disco.Models.Services.Devices.Importing;
using System;

namespace Disco.Services.Devices.Importing
{
    public class DeviceImportColumn : IDeviceImportColumn
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public DeviceImportFieldTypes Type { get; set; }
        public Type Handler { get; set; }

        public IDeviceImportField GetHandlerInstance()
        {
            if (Handler == null)
                throw new InvalidOperationException($"No field handler available for this type {Type.ToString()}.");

            return (IDeviceImportField)Activator.CreateInstance(Handler);
        }
    }
}
