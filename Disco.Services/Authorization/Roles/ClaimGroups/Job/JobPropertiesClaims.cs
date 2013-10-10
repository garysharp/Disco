using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Authorization.Roles.ClaimGroups.Job
{
    [ClaimDetails("Job Properties", "Permissions related to Job Properties")]
    public class JobPropertiesClaims : BaseRoleClaimGroup
    {
        public JobPropertiesClaims()
        {
            this.WarrantyProperties = new JobWarrantyPropertiesClaims();
            this.NonWarrantyProperties = new JobNonWarrantyPropertiesClaims();
        }

        public JobWarrantyPropertiesClaims WarrantyProperties { get; set; }
        public JobNonWarrantyPropertiesClaims NonWarrantyProperties { get; set; }

        [ClaimDetails("Device Held Property", "Can update property")]
        public bool DeviceHeld { get; set; }
        [ClaimDetails("Device Ready For Return Property", "Can update property")]
        public bool DeviceReadyForReturn { get; set; }
        [ClaimDetails("Device Returned Property", "Can update property")]
        public bool DeviceReturned { get; set; }
        [ClaimDetails("Waiting For User Action Property", "Can update property")]
        public bool WaitingForUserAction { get; set; }
        [ClaimDetails("Not Waiting For User Action Property", "Can update property")]
        public bool NotWaitingForUserAction { get; set; }

        [ClaimDetails("Flags Property", "Can update property")]
        public bool Flags { get; set; }

        [ClaimDetails("Expected Closed Date Property", "Can update property")]
        public bool ExpectedClosedDate { get; set; }
        [ClaimDetails("Device Held Location Property", "Can update property")]
        public bool DeviceHeldLocation { get; set; }
    }
}
