using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Authorization.Roles.ClaimGroups.Device
{
    [ClaimDetails("Device Properties", "Permissions related to Device Properties")]
    public class DevicePropertiesClaims : BaseRoleClaimGroup
    {
        [ClaimDetails("Device Profile Property", "Can update property")]
        public bool DeviceProfile { get; set; }

        [ClaimDetails("Device Batch Property", "Can update property")]
        public bool DeviceBatch { get; set; }

        [ClaimDetails("Asset Number Property", "Can update property")]
        public bool AssetNumber { get; set; }

        [ClaimDetails("Location Property", "Can update property")]
        public bool Location { get; set; }

        [ClaimDetails("Detail Properties", "Can update detail properties")]
        public bool Details { get; set; }
    }
}
