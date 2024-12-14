using Disco.Models.UI.Config.DeviceProfile;

namespace Disco.Web.Areas.Config.Models.DeviceProfile
{
    public class CreateModel : ConfigDeviceProfileCreateModel
    {
        public Disco.Models.Repository.DeviceProfile DeviceProfile { get; set; }
    }
}