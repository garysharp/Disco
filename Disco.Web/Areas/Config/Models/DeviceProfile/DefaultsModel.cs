using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disco.Web.Areas.Config.Models.DeviceProfile
{
    public class DefaultsModel
    {
        public List<Disco.Models.Repository.DeviceProfile> DeviceProfiles { get; set; }
        public List<Disco.Models.Repository.DeviceProfile> DeviceProfilesAndNone { get; set; }
        public int Default { get; set; }
        public int DefaultAddDeviceOffline { get; set; }
    }
}