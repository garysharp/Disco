namespace Disco.Services.Authorization.Roles.ClaimGroups.Configuration.UserFlag
{
    [ClaimDetails("Device Flags", "Permissions related to Device Flags")]
    public class DeviceFlagClaims : BaseRoleClaimGroup
    {
        [ClaimDetails("Configure Device Flags", "Can configure device flags")]
        public bool Configure { get; set; }

        [ClaimDetails("Create Device Flags", "Can create device flags")]
        public bool Create { get; set; }

        [ClaimDetails("Delete Device Flags", "Can delete device flags")]
        public bool Delete { get; set; }
        [ClaimDetails("Export Device Flag Assignments", "Can export user device assignments")]
        public bool Export { get; set; }

        [ClaimDetails("Show Device Flags", "Can show device flags")]
        public bool Show { get; set; }
    }
}
