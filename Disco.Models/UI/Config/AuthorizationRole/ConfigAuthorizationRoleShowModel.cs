using Disco.Models.Services.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.UI.Config.AuthorizationRole
{
    public interface ConfigAuthorizationRoleShowModel : BaseUIModel
    {
        IRoleToken Token { get; set; }
    }
}
