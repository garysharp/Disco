namespace Disco.Services.Authorization.Roles.ClaimGroups.User
{
    [ClaimDetails("User", "Permissions related to Users")]
    public class UserClaims : BaseRoleClaimGroup
    {
        public UserClaims()
        {
            Actions = new UserActionsClaims();
        }

        [ClaimDetails("Search Users", "Can search users")]
        public bool Search { get; set; }

        [ClaimDetails("Show Users", "Can show users")]
        public bool Show { get; set; }

        [ClaimDetails("Show Users Details", "Can show users contact and personal details")]
        public bool ShowDetails { get; set; }

        [ClaimDetails("Show Comments", "Can show user comments")]
        public bool ShowComments { get; set; }

        [ClaimDetails("Show Attachments", "Can show user attachments")]
        public bool ShowAttachments { get; set; }

        [ClaimDetails("Show Device Assignment History", "Can show the device assignment history for users")]
        public bool ShowAssignmentHistory { get; set; }

        [ClaimDetails("Show Device Assignments", "Can show the current device assignments users")]
        public bool ShowAssignments { get; set; }

        [ClaimDetails("Show Users Jobs", "Can show jobs associated with users")]
        public bool ShowJobs { get; set; }

        [ClaimDetails("Show Users Flag Assignments", "Can show flags associated with users")]
        public bool ShowFlagAssignments { get; set; }

        [ClaimDetails("Show Users Authorization", "Can show authorization permissions associated with users")]
        public bool ShowAuthorization { get; set; }

        public UserActionsClaims Actions { get; set; }
    }
}
