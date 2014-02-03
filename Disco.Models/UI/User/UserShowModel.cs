using Disco.Models.Services.Authorization;
using Disco.Models.Services.Jobs.JobLists;
using System.Collections.Generic;

namespace Disco.Models.UI.User
{
    public interface UserShowModel : BaseUIModel
    {
        Disco.Models.Repository.User User { get; set; }
        JobTableModel Jobs { get; set; }
        List<Disco.Models.Repository.DocumentTemplate> DocumentTemplates { get; set; }
        IAuthorizationToken AuthorizationToken { get; set; }
        IClaimNavigatorItem ClaimNavigator { get; set; }
    }
}