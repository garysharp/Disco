using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.UI.Config.DeviceModel
{
    public interface ConfigDeviceModelIndexModel : BaseUIModel
    {
        List<ConfigDeviceModelIndexModelItem> DeviceModels { get; set; }
    }
}
