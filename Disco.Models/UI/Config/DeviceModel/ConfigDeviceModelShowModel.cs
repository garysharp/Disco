using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.UI.Config.DeviceModel
{
    public interface ConfigDeviceModelShowModel : BaseUIModel
    {
        Disco.Models.Repository.DeviceModel DeviceModel { get; set; }

        ConfigDeviceModelComponentsModel DeviceComponentsModel { get; set; }

        bool CanDelete { get; set; }
    }
}
