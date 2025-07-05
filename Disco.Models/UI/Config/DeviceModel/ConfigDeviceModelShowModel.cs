using Disco.Models.UI.Config.Shared;

namespace Disco.Models.UI.Config.DeviceModel
{
    public interface ConfigDeviceModelShowModel : BaseUIModel, ConfigSharedDeviceGroupDocumentTemplateBulkGenerate
    {
        Disco.Models.Repository.DeviceModel DeviceModel { get; set; }

        ConfigDeviceModelComponentsModel DeviceComponentsModel { get; set; }

        int DeviceCount { get; set; }
        int DeviceDecommissionedCount { get; set; }

        bool CanDelete { get; set; }
        bool CanDecommission { get; set; }
    }
}
