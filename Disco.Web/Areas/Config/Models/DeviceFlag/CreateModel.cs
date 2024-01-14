using Disco.Models.UI.Config.DeviceFlag;

namespace Disco.Web.Areas.Config.Models.DeviceFlag
{
    public class CreateModel : ConfigDeviceFlagCreateModel
    {
        public Disco.Models.Repository.DeviceFlag DeviceFlag { get; set; }
    }
}
