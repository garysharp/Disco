using Disco.Models.UI.Config.DeviceModel;
using Disco.Services.Plugins;
using Disco.Web.Areas.Config.Models.Shared;
using System.Collections.Generic;

namespace Disco.Web.Areas.Config.Models.DeviceModel
{
    public class ShowModel : DeviceGroupDocumentTemplateBulkGenerateModel, ConfigDeviceModelShowModel
    {
        public Disco.Models.Repository.DeviceModel DeviceModel { get; set; }

        public ConfigDeviceModelComponentsModel DeviceComponentsModel { get; set; }

        public List<PluginFeatureManifest> WarrantyProviders { get; set; }
        public List<PluginFeatureManifest> RepairProviders { get; set; }

        public int DeviceCount { get; set; }
        public int DeviceDecommissionedCount { get; set; }

        public bool CanDelete { get; set; }
        public bool CanDecommission { get; set; }

        public override int DeviceGroupId => DeviceModel.Id;
    }
}