using Disco.Models.UI.Config.DeviceModel;
using System.ComponentModel.DataAnnotations;

namespace Disco.Web.Areas.Config.Models.DeviceModel
{
    public class CreateModel : ConfigDeviceModelCreateModel
    {
        [Required, StringLength(500)]
        public string Description { get; set; }
        [StringLength(200)]
        public string Manufacturer { get; set; }
        [StringLength(200)]
        public string ManufacturerModel { get; set; }
    }
}
