using Disco.Models.UI.Config.AuthorizationRole;

namespace Disco.Web.Areas.Config.Models.AuthorizationRole
{
    public class CreateModel : ConfigAuthorizationRoleCreateModel
    {
        public Disco.Models.Repository.AuthorizationRole AuthorizationRole { get; set; }
    }
}