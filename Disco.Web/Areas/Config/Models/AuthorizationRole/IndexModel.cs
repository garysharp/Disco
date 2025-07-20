using Disco.Models.Services.Authorization;
using Disco.Models.UI.Config.AuthorizationRole;
using Disco.Web.Areas.API.Models.Shared;
using System.Collections.Generic;

namespace Disco.Web.Areas.Config.Models.AuthorizationRole
{
    public class IndexModel : ConfigAuthorizationRoleIndexModel
    {
        public List<IRoleToken> Tokens { get; set; }
        public List<SubjectDescriptorModel> AdministratorSubjects { get; set; }
    }
}