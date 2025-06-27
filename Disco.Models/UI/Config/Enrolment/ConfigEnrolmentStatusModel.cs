using System.Collections.Generic;

namespace Disco.Models.UI.Config.Enrolment
{
    public interface ConfigEnrolmentStatusModel : BaseUIModel
    {
        int DefaultDeviceProfileId { get; set; }
        List<Repository.DeviceProfile> DeviceProfiles { get; set; }
        List<Repository.DeviceBatch> DeviceBatches { get; set; }
    }
}
