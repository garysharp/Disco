using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Disco.Models.UI.Device;

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