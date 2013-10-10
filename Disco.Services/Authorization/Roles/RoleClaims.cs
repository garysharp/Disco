using Disco.Services.Authorization.Roles.ClaimGroups;
using Disco.Services.Authorization.Roles.ClaimGroups.Configuration;
using Disco.Services.Authorization.Roles.ClaimGroups.Device;
using Disco.Services.Authorization.Roles.ClaimGroups.Job;
using Disco.Services.Authorization.Roles.ClaimGroups.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Authorization.Roles
{
    public class RoleClaims : BaseRoleClaimGroup
    {
        public RoleClaims()
        {
            this.Config = new ConfigClaims();

            this.Job = new JobClaims();
            this.Device = new DeviceClaims();
            this.User = new UserClaims();
        }

        [ClaimDetails("Computer Account", "Represents a computer account", true)]
        public bool ComputerAccount { get; set; }

        [ClaimDetails("Disco Administrator Account", "Represents a Disco Administrator account", true)]
        public bool DiscoAdminAccount { get; set; }

        public ConfigClaims Config { get; set; }

        public JobClaims Job { get; set; }

        public DeviceClaims Device { get; set; }

        public UserClaims User { get; set; }
    }
}
