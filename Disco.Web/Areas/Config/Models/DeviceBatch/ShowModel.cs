using Disco.Models.UI.Config.DeviceBatch;
using Disco.Services.Devices.ManagedGroups;
using Disco.Web.Areas.Config.Models.Shared;
using System.Collections.Generic;

namespace Disco.Web.Areas.Config.Models.DeviceBatch
{
    public class ShowModel : DeviceGroupDocumentTemplateBulkGenerateModel, ConfigDeviceBatchShowModel
    {
        public Disco.Models.Repository.DeviceBatch DeviceBatch { get; set; }

        public Disco.Models.Repository.DeviceModel DefaultDeviceModel { get; set; }
        
        public List<Disco.Models.Repository.DeviceModel> DeviceModels { get; set; }
        public List<ConfigDeviceBatchShowModelMembership> DeviceModelMembers { get; set; }

        public DeviceBatchAssignedUsersManagedGroup AssignedUsersLinkedGroup { get; set; }
        public DeviceBatchDevicesManagedGroup DevicesLinkedGroup { get; set; }

        public int DeviceCount { get; set; }
        public int DeviceDecommissionedCount { get; set; }
        public bool CanDelete { get; set; }

        public override int DeviceGroupId => DeviceBatch.Id;
    }
}