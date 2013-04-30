using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.UI.Device
{
    public interface DeviceAddOfflineModel : BaseUIModel
    {
        Disco.Models.Repository.Device Device { get; set; }
        List<Disco.Models.Repository.DeviceProfile> DeviceProfiles { get; set; }
        List<Disco.Models.Repository.DeviceBatch> DeviceBatches { get; set; }
        int DefaultDeviceProfileId { get; set; }
    }
}
