using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.UI.Config.DeviceBatch
{
    public interface ConfigDeviceBatchShowModel : BaseUIModel
    {
        Disco.Models.Repository.DeviceBatch DeviceBatch { get; set; }
        List<Disco.Models.Repository.DeviceModel> DeviceModels { get; set; }
        int DeviceCount { get; set; }
        int DeviceDecommissionedCount { get; set; }
        bool CanDelete { get; set; }
    }
}
