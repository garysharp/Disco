using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Disco.Web.Extensions;
using Disco.Models.UI.Config.DeviceProfile;

namespace Disco.Web.Areas.Config.Models.DeviceProfile
{
    public class _IndexModelItem : ConfigDeviceProfileIndexModelItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public int? Address { get; set; }
        public string AddressName { get; set; }
        public string Description { get; set; }
        public Disco.Models.Repository.DeviceProfile.DistributionTypes DistributionType { get; set; }

        public int DeviceCount { get; set; }
        public int DeviceDecommissionedCount { get; set; }
    }
}