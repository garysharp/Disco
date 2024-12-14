using Disco.Models.UI.Config.DeviceModel;
using System.Collections.Generic;

namespace Disco.Web.Areas.Config.Models.DeviceModel
{
    public class ComponentsModel : ConfigDeviceModelComponentsModel
    {
        public int? DeviceModelId { get; set; }
        public List<Disco.Models.Repository.DeviceComponent> DeviceComponents { get; set; }

        public List<Disco.Models.Repository.JobSubType> JobSubTypes { get; set; }
    }
}