using Disco.Models.UI.Config.Enrolment;
using System.Collections.Generic;

namespace Disco.Web.Areas.Config.Models.Enrolment
{
    public class StatusModel : ConfigEnrolmentStatusModel
    {
        public int DefaultDeviceProfileId { get; set; }
        public List<Disco.Models.Repository.DeviceProfile> DeviceProfiles { get; set; }
        public List<Disco.Models.Repository.DeviceBatch> DeviceBatches { get; set; }
    }
}
