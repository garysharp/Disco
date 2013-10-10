using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Authorization.Roles.ClaimGroups.Job
{
    [ClaimDetails("Warranty Properties", "Permissions related to Warranty Job Properties")]
    public class JobWarrantyPropertiesClaims : BaseRoleClaimGroup
    {
        [ClaimDetails("Warranty Completed Property", "Can update property")]
        public bool WarrantyCompleted { get; set; }

        [ClaimDetails("External Name Property", "Can update property")]
        public bool ExternalName { get; set; }
        [ClaimDetails("External Logged Date Property", "Can update property")]
        public bool ExternalLoggedDate { get; set; }
        [ClaimDetails("External Reference Property", "Can update property")]
        public bool ExternalReference { get; set; }
        [ClaimDetails("External Completed Date Property", "Can update property")]
        public bool ExternalCompletedDate { get; set; }

        [ClaimDetails("Provider Details", "Can access warranty provider details")]
        public bool ProviderDetails { get; set; }
    }
}
