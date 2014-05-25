using Disco.Models.Repository;
using System.Collections.Generic;

namespace Disco.Services.Devices.Importing
{
    internal interface IDeviceImportCache
    {
        Device FindDevice(string DeviceSerialNumber);

        IEnumerable<Device> Devices { get; }
        IEnumerable<DeviceModel> DeviceModels { get; }
        IEnumerable<DeviceProfile> DeviceProfiles { get; }
        IEnumerable<DeviceBatch> DeviceBatches { get; }
    }
}