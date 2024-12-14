using Disco.Models.UI.Config.DeviceProfile;
using System.Collections.Generic;

namespace Disco.Web.Areas.Config.Models.DeviceProfile
{
    public class DefaultsModel : ConfigDeviceProfileDefaultsModel
    {
        public List<Disco.Models.Repository.DeviceProfile> DeviceProfiles { get; set; }
        public List<Disco.Models.Repository.DeviceProfile> DeviceProfilesAndNone { get; set; }
        public int Default { get; set; }
        public int DefaultAddDeviceOffline { get; set; }
    }
}