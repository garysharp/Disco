using Disco.Models.UI.Config.DeviceProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disco.Web.Areas.Config.Models.DeviceProfile
{
    public class CreateModel : ConfigDeviceProfileCreateModel
    {
        public Disco.Models.Repository.DeviceProfile DeviceProfile { get; set; }
    }
}