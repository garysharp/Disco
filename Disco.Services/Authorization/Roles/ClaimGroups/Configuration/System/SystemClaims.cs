namespace Disco.Services.Authorization.Roles.ClaimGroups.Configuration.System
{
    [ClaimDetails("System", "Permissions related to System Configuration")]
    public class SystemClaims : BaseRoleClaimGroup
    {
        [ClaimDetails("Show System Configuration", "Can show the system configuration")]
        public bool Show { get; set; }

        [ClaimDetails("Configure Proxy Settings", "Can configure the proxy settings")]
        public bool ConfigureProxy { get; set; }
        
        [ClaimDetails("Configure Email Settings", "Can configure the email settings")]
        public bool ConfigureEmail { get; set; }

        [ClaimDetails("Configure Active Directory Settings", "Can configure the Active Directory interoperability settings")]
        public bool ConfigureActiveDirectory { get; set; }
    }
}
