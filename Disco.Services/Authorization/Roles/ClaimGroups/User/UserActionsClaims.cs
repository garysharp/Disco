namespace Disco.Services.Authorization.Roles.ClaimGroups.User
{
    [ClaimDetails("Actions", "Permissions related to User Actions")]
    public class UserActionsClaims : BaseRoleClaimGroup
    {
        [ClaimDetails("Add Comments", "Can add user comments")]
        public bool AddComments { get; set; }
        [ClaimDetails("Remove Any Comments", "Can remove any user comments")]
        public bool RemoveAnyComments { get; set; }
        [ClaimDetails("Remove Own Comments", "Can remove own user comments")]
        public bool RemoveOwnComments { get; set; }

        [ClaimDetails("Add Attachments", "Can add attachments to users")]
        public bool AddAttachments { get; set; }
        [ClaimDetails("Remove Any Attachments", "Can remove any attachments from users")]
        public bool RemoveAnyAttachments { get; set; }
        [ClaimDetails("Remove Own Attachments", "Can remove own attachments from users")]
        public bool RemoveOwnAttachments { get; set; }

        [ClaimDetails("Generate Documents", "Can generate documents for users")]
        public bool GenerateDocuments { get; set; }

        [ClaimDetails("Add User Flags", "Can add user flags")]
        public bool AddFlags { get; set; }
        [ClaimDetails("Remove User Flags", "Can remove user flags")]
        public bool RemoveFlags { get; set; }
        [ClaimDetails("Edit User Flags", "Can edit user flags")]
        public bool EditFlags { get; set; }
    }
}