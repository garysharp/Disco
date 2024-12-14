using System.Collections.Generic;

namespace Disco.Models.UI.Config.DeviceModel
{
    public interface ConfigDeviceModelComponentsModel : BaseUIModel
    {
        int? DeviceModelId { get; set; }
        List<Disco.Models.Repository.DeviceComponent> DeviceComponents { get; set; }

        List<Disco.Models.Repository.JobSubType> JobSubTypes { get; set; }
    }
}
