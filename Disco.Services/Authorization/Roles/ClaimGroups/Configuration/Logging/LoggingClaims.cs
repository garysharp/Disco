using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Authorization.Roles.ClaimGroups.Configuration.Logging
{
    [ClaimDetails("Logging", "Permissions related to Logging")]
    public class LoggingClaims : BaseRoleClaimGroup
    {
        [ClaimDetails("Show Logging", "Can show logging")]
        public bool Show { get; set; }
    }
}
