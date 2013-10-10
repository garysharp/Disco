using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Disco.Data.Repository;
using Disco.Models.UI.Config.AuthorizationRole;
using Disco.Models.Authorization;

namespace Disco.Web.Areas.Config.Models.AuthorizationRole
{
    public class IndexModel : ConfigAuthorizationRoleIndexModel
    {
        public List<IRoleToken> Tokens { get; set; }
    }
}