using Disco.Models.UI.Device;
using System.Collections.Generic;

namespace Disco.Web.Models.Device
{
    public class AddOfflineModel : DeviceAddOfflineModel
    {
        public Disco.Models.Repository.Device Device { get; set; }
        public List<Disco.Models.Repository.DeviceProfile> DeviceProfiles { get; set; }
        public List<Disco.Models.Repository.DeviceBatch> DeviceBatches { get; set; }
        public int DefaultDeviceProfileId { get; set; }
    }
}