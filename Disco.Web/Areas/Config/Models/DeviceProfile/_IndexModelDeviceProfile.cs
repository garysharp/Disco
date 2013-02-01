using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Disco.Web.Extensions;

namespace Disco.Web.Areas.Config.Models.DeviceProfile
{
    public class _IndexModelDeviceProfile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public int? Address { get; set; }
        //public string AddressShortName { get; set; }
        public string AddressName { get; set; }
        public string Description { get; set; }
        public int DistributionTypeId { get; set; }
        
        public string DistributionType
        {
            get
            {
                return Enum.GetName(typeof(Disco.Models.Repository.DeviceProfile.DistributionTypes), this.DistributionTypeId);
            }
        }

        public int DeviceCount { get; set; }
        public int DeviceDecommissionedCount { get; set; }
    }
}