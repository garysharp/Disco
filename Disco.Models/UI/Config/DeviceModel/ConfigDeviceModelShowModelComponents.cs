using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.UI.Config.DeviceModel
{
    public interface ConfigDeviceModelComponentsModel : BaseUIModel
    {
        int? DeviceModelId { get; set; }
        List<Disco.Models.Repository.DeviceComponent> DeviceComponents { get; set; }

        List<Disco.Models.Repository.JobSubType> JobSubTypes { get; set; }
    }
}
