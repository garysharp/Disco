using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Disco.Services.Plugins;

namespace Disco.Web.Areas.Config.Models.DeviceModel
{
    public class ShowModel
    {
        public Disco.Models.Repository.DeviceModel DeviceModel { get; set; }

        public Models.DeviceModel.DeviceComponentsModel DeviceComponentsModel { get; set; }
        
        public List<PluginFeatureManifest> WarrantyProviders { get; set; }

        public bool CanDelete { get; set; }
    }
}