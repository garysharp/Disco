using System.Collections.Generic;

namespace Disco.Models.UI.Config.DeviceModel
{
    public interface ConfigDeviceModelIndexModel : BaseUIModel
    {
        List<ConfigDeviceModelIndexModelItem> DeviceModels { get; set; }
    }
}
