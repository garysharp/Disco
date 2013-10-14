using Disco.Models.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.UI.User
{
    public interface UserShowModel : BaseUIModel
    {
        Disco.Models.Repository.User User { get; set; }
        Disco.Models.BI.Job.JobTableModel Jobs { get; set; }
        List<Disco.Models.Repository.DocumentTemplate> DocumentTemplates { get; set; }
        IAuthorizationToken AuthorizationToken { get; set; }
        IClaimNavigatorItem ClaimNavigator { get; set; }
    }
}
