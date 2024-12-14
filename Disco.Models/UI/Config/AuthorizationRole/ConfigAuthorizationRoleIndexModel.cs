using Disco.Models.Services.Authorization;
using System.Collections.Generic;

namespace Disco.Models.UI.Config.AuthorizationRole
{
    public interface ConfigAuthorizationRoleIndexModel : BaseUIModel
    {
        List<IRoleToken> Tokens { get; set; }
    }
}
