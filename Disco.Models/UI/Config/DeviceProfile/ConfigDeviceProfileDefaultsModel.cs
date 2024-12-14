using System.Collections.Generic;

namespace Disco.Models.UI.Config.DeviceProfile
{
    public interface ConfigDeviceProfileDefaultsModel : BaseUIModel
    {
        List<Disco.Models.Repository.DeviceProfile> DeviceProfiles { get; set; }
        List<Disco.Models.Repository.DeviceProfile> DeviceProfilesAndNone { get; set; }
        int Default { get; set; }
        int DefaultAddDeviceOffline { get; set; }
    }
}
