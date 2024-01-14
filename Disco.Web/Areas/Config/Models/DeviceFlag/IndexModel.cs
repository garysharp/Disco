using Disco.Models.UI.Config.DeviceFlag;
using System.Collections.Generic;

namespace Disco.Web.Areas.Config.Models.DeviceFlag
{
    public class IndexModel : ConfigDeviceFlagIndexModel
    {
        public Dictionary<Disco.Models.Repository.DeviceFlag, int> DeviceFlags { get; set; }
    }
}
