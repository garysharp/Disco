using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Authorization.Roles.ClaimGroups.Job
{
    [ClaimDetails("Types", "Permissions related to Job Types")]
    public class JobTypesClaims : BaseRoleClaimGroup
    {
        [ClaimDetails("Show Hardware - Miscellaneous Jobs", "Can show jobs of this type")]
        public bool ShowHMisc { get; set; }
        [ClaimDetails("Show Hardware - Non-Warranty Jobs", "Can show jobs of this type")]
        public bool ShowHNWar { get; set; }
        [ClaimDetails("Show Hardware - Warranty Jobs", "Can show jobs of this type")]
        public bool ShowHWar { get; set; }

        [ClaimDetails("Show Software - Application Jobs", "Can show jobs of this type")]
        public bool ShowSApp { get; set; }
        [ClaimDetails("Show Software - Reimage Jobs", "Can show jobs of this type")]
        public bool ShowSImg { get; set; }
        [ClaimDetails("Show Software - Operating System Jobs", "Can show jobs of this type")]
        public bool ShowSOS { get; set; }
        
        [ClaimDetails("Show User Management Jobs", "Can show jobs of this type")]
        public bool ShowUMgmt { get; set; }
    }
}
