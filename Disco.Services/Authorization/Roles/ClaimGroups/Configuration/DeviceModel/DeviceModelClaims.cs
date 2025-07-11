namespace Disco.Services.Authorization.Roles.ClaimGroups.Configuration.DeviceModel
{
    [ClaimDetails("Device Models", "Permissions related to Device Models")]
    public class DeviceModelClaims : BaseRoleClaimGroup
    {
        [ClaimDetails("Configure Device Models", "Can configure device models")]
        public bool Configure { get; set; }

        [ClaimDetails("Configure Device Model Components", "Can configure device model components")]
        public bool ConfigureComponents { get; set; }

        [ClaimDetails("Create Custom Device Models", "Can create custom device models")]
        public bool CreateCustom { get; set; }

        [ClaimDetails("Delete Device Models", "Can delete device models")]
        public bool Delete { get; set; }

        [ClaimDetails("Show Device Models", "Can show device models")]
        public bool Show { get; set; }
    }
}
