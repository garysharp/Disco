using Disco.Services.Authorization.Roles.ClaimGroups.Configuration;
using Disco.Services.Authorization.Roles.ClaimGroups.Device;
using Disco.Services.Authorization.Roles.ClaimGroups.Job;
using Disco.Services.Authorization.Roles.ClaimGroups.User;

namespace Disco.Services.Authorization.Roles
{
    public class RoleClaims : BaseRoleClaimGroup
    {
        public RoleClaims()
        {
            Config = new ConfigClaims();

            Job = new JobClaims();
            Device = new DeviceClaims();
            User = new UserClaims();
        }

        [ClaimDetails("Computer Account", "Represents a computer account", true)]
        public bool ComputerAccount { get; set; }

        [ClaimDetails("Disco Administrator Account", "Represents a Disco ICT Administrator account", true)]
        public bool DiscoAdminAccount { get; set; }

        public ConfigClaims Config { get; set; }

        public JobClaims Job { get; set; }

        public DeviceClaims Device { get; set; }

        public UserClaims User { get; set; }
    }
}
