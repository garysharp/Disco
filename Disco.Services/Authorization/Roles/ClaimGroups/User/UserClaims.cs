using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Authorization.Roles.ClaimGroups.User
{
    [ClaimDetails("User", "Permissions related to Users")]
    public class UserClaims : BaseRoleClaimGroup
    {
        public UserClaims()
        {
            this.Actions = new UserActionsClaims();
        }

        [ClaimDetails("Search Users", "Can search users")]
        public bool Search { get; set; }

        [ClaimDetails("Show Users", "Can show users")]
        public bool Show { get; set; }

        [ClaimDetails("Show Attachments", "Can show user attachments")]
        public bool ShowAttachments { get; set; }

        [ClaimDetails("Show Device Assignment History", "Can show the device assignment history for users")]
        public bool ShowAssignmentHistory { get; set; }

        [ClaimDetails("Show Users Jobs", "Can show jobs associated with users")]
        public bool ShowJobs { get; set; }

        [ClaimDetails("Show Users Authorization", "Can show authorization permissions associated with users")]
        public bool ShowAuthorization { get; set; }

        public UserActionsClaims Actions { get; set; }
    }
}
