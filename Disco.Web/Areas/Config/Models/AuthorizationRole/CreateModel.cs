using Disco.Models.UI.Config.AuthorizationRole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disco.Web.Areas.Config.Models.AuthorizationRole
{
    public class CreateModel : ConfigAuthorizationRoleCreateModel
    {
        public Disco.Models.Repository.AuthorizationRole AuthorizationRole { get; set; }
    }
}