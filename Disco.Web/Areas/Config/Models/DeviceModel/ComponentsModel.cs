using Disco.Models.UI.Config.DeviceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disco.Web.Areas.Config.Models.DeviceModel
{
    public class ComponentsModel : ConfigDeviceModelComponentsModel
    {
        public int? DeviceModelId { get; set; }
        public List<Disco.Models.Repository.DeviceComponent> DeviceComponents { get; set; }

        public List<Disco.Models.Repository.JobSubType> JobSubTypes { get; set; }
    }
}