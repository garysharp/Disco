using System.Collections.Generic;

namespace Disco.Models.UI.Device
{
    public interface DeviceAddOfflineModel : BaseUIModel
    {
        Repository.Device Device { get; set; }
        List<Repository.DeviceProfile> DeviceProfiles { get; set; }
        List<Repository.DeviceBatch> DeviceBatches { get; set; }
        int DefaultDeviceProfileId { get; set; }
    }
}
