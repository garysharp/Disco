using System.Collections.Generic;

namespace Disco.Models.UI.Config.DeviceProfile
{
    public interface ConfigDeviceProfileDefaultsModel : BaseUIModel
    {
        List<Repository.DeviceProfile> DeviceProfiles { get; set; }
        List<Repository.DeviceProfile> DeviceProfilesAndNone { get; set; }
        int Default { get; set; }
        int DefaultAddDeviceOffline { get; set; }
    }
}
