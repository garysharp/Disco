using Disco.Models.UI.Config.DeviceFlag;
using System.ComponentModel.DataAnnotations;

namespace Disco.Web.Areas.Config.Models.DeviceFlag
{
    public class CreateModel : ConfigDeviceFlagCreateModel
    {
        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(500), DataType(DataType.MultilineText)]
        public string Description { get; set; }
    }
}
