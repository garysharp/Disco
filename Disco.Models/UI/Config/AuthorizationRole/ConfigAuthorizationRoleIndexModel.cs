using Disco.Models.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.UI.Config.AuthorizationRole
{
    public interface ConfigAuthorizationRoleIndexModel : BaseUIModel
    {
        List<IRoleToken> Tokens { get; set; }
    }
}
