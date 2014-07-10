using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Disco.Services.Plugins;
using Disco.Models.UI.Config.DeviceModel;

namespace Disco.Web.Areas.Config.Models.DeviceModel
{
    public class ShowModel : ConfigDeviceModelShowModel
    {
        public Disco.Models.Repository.DeviceModel DeviceModel { get; set; }

        public ConfigDeviceModelComponentsModel DeviceComponentsModel { get; set; }

        public List<PluginFeatureManifest> WarrantyProviders { get; set; }
        public List<PluginFeatureManifest> RepairProviders { get; set; }

        public int DeviceCount { get; set; }
        public int DeviceDecommissionedCount { get; set; }

        public bool CanDelete { get; set; }
    }
}