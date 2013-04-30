using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.UI.Config.DeviceProfile
{
    public interface ConfigDeviceProfileDefaultsModel : BaseUIModel
    {
        List<Disco.Models.Repository.DeviceProfile> DeviceProfiles { get; set; }
        List<Disco.Models.Repository.DeviceProfile> DeviceProfilesAndNone { get; set; }
        int Default { get; set; }
        int DefaultAddDeviceOffline { get; set; }
    }
}
