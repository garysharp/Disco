using Disco.Models.UI.Config.AuthorizationRole;
using System.ComponentModel.DataAnnotations;

namespace Disco.Web.Areas.Config.Models.AuthorizationRole
{
    public class CreateModel : ConfigAuthorizationRoleCreateModel
    {
        [Required, StringLength(100)]
        public string Name { get; set; }
    }
}