using System.Collections.Generic;

namespace Disco.Models.UI.Config.DeviceFlag
{
    public interface ConfigDeviceFlagIndexModel : BaseUIModel
    {
        Dictionary<Repository.DeviceFlag, int> DeviceFlags { get; set; }
    }
}
