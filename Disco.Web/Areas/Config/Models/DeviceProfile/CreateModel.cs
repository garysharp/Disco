using Disco.Models.UI.Config.DeviceProfile;
using System.ComponentModel.DataAnnotations;

namespace Disco.Web.Areas.Config.Models.DeviceProfile
{
    public class CreateModel : ConfigDeviceProfileCreateModel
    {
        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required, StringLength(10)]
        public string ShortName { get; set; }

        [StringLength(500), DataType(DataType.MultilineText)]
        public string Description { get; set; }
    }
}