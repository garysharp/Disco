using Disco.Models.UI.Config.Shared;
using System.Collections.Generic;

namespace Disco.Models.UI.Config.DeviceBatch
{
    public interface ConfigDeviceBatchShowModel : BaseUIModel, ConfigSharedDeviceGroupDocumentTemplateBulkGenerate
    {
        Repository.DeviceBatch DeviceBatch { get; set; }

        Repository.DeviceModel DefaultDeviceModel { get; set; }

        List<Repository.DeviceModel> DeviceModels { get; set; }

        List<ConfigDeviceBatchShowModelMembership> DeviceModelMembers { get; set; }

        int DeviceCount { get; set; }
        int DeviceDecommissionedCount { get; set; }
        bool CanDelete { get; set; }
    }
}
