using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Services.Authorization.Roles.ClaimGroups.User
{
    [ClaimDetails("Actions", "Permissions related to User Actions")]
    public class UserActionsClaims : BaseRoleClaimGroup
    {
        [ClaimDetails("Add Attachments", "Can add attachments to users")]
        public bool AddAttachments { get; set; }
        [ClaimDetails("Remove Any Attachments", "Can remove any attachments from users")]
        public bool RemoveAnyAttachments { get; set; }
        [ClaimDetails("Remove Own Attachments", "Can remove own attachments from users")]
        public bool RemoveOwnAttachments { get; set; }

        [ClaimDetails("Generate Documents", "Can generate documents for users")]
        public bool GenerateDocuments { get; set; }
    }
}
