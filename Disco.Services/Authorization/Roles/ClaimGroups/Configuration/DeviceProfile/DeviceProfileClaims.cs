namespace Disco.Services.Authorization.Roles.ClaimGroups.Configuration.DeviceProfile
{
    [ClaimDetails("Device Profiles", "Permissions related to Device Profiles")]
    public class DeviceProfileClaims : BaseRoleClaimGroup
    {
        [ClaimDetails("Configure Device Profiles", "Can configure device profiles")]
        public bool Configure { get; set; }

        [ClaimDetails("Configure Computer Name Templates", "Can configure computer name templates for device profiles")]
        public bool ConfigureComputerNameTemplate { get; set; }

        [ClaimDetails("Configure Default Device Profiles", "Can configure default device profiles")]
        public bool ConfigureDefaults { get; set; }

        [ClaimDetails("Create Device Profiles", "Can create device profiles")]
        public bool Create { get; set; }

        [ClaimDetails("Delete Device Profiles", "Can delete device profiles")]
        public bool Delete { get; set; }

        [ClaimDetails("Show Device Profiles", "Can show device profiles")]
        public bool Show { get; set; }
    }
}
