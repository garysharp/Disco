using System.Collections.Generic;

namespace Disco.Models.UI.Config.DeviceProfile
{
    public interface ConfigDeviceProfileIndexModel : BaseUIModel
    {
        List<ConfigDeviceProfileIndexModelItem> DeviceProfiles { get; set; }
    }
}
