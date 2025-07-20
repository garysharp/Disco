using System.Collections.Generic;

namespace Disco.Models.UI.Config.DeviceModel
{
    public interface ConfigDeviceModelComponentsModel : BaseUIModel
    {
        int? DeviceModelId { get; set; }
        List<Repository.DeviceComponent> DeviceComponents { get; set; }

        List<Repository.JobSubType> JobSubTypes { get; set; }
    }
}
