using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Authorization.Roles.ClaimGroups.Configuration.Origanisation
{
    [ClaimDetails("Organisation Details", "Permissions related to the Organisation Details")]
    public class OrganisationClaims : BaseRoleClaimGroup
    {
        [ClaimDetails("Show Organisation Details", "Can show the organisation details")]
        public bool Show { get; set; }

        [ClaimDetails("Configure Name", "Can configure the organisation name")]
        public bool ConfigureName { get; set; }

        [ClaimDetails("Configure Logo", "Can configure the organisation logo")]
        public bool ConfigureLogo { get; set; }

        [ClaimDetails("Configure Multi-Site Mode", "Can configure multi-site mode")]
        public bool ConfigureMultiSiteMode { get; set; }

        [ClaimDetails("Configure Addresses", "Can configure organisation addresses")]
        public bool ConfigureAddresses { get; set; }
    }
}
