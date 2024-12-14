using Disco.Models.Services.Authorization;

namespace Disco.Models.UI.Config.AuthorizationRole
{
    public interface ConfigAuthorizationRoleShowModel : BaseUIModel
    {
        IRoleToken Token { get; set; }
    }
}
