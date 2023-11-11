namespace Disco.Services.Authorization.Roles.ClaimGroups.Configuration.UserFlag
{
    [ClaimDetails("User Flags", "Permissions related to User Flags")]
    public class UserFlagClaims : BaseRoleClaimGroup
    {
        [ClaimDetails("Configure User Flags", "Can configure user flags")]
        public bool Configure { get; set; }

        [ClaimDetails("Create User Flags", "Can create user flags")]
        public bool Create { get; set; }

        [ClaimDetails("Delete User Flags", "Can delete user flags")]
        public bool Delete { get; set; }
        [ClaimDetails("Export User Flag Assignments", "Can export user flag assignments")]
        public bool Export { get; set; }

        [ClaimDetails("Show User Flags", "Can show user flags")]
        public bool Show { get; set; }
    }
}