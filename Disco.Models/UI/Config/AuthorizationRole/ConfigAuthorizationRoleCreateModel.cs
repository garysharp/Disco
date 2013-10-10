using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.UI.Config.AuthorizationRole
{
    public interface ConfigAuthorizationRoleCreateModel : BaseUIModel
    {
        Models.Repository.AuthorizationRole AuthorizationRole { get; set; }
    }
}
