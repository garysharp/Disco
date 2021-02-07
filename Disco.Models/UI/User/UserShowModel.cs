using Disco.Models.Repository;
using Disco.Models.Services.Authorization;
using Disco.Models.Services.Documents;
using Disco.Models.Services.Jobs.JobLists;
using Disco.Models.Services.Plugins.Details;
using System.Collections.Generic;

namespace Disco.Models.UI.User
{
    public interface UserShowModel : BaseUIModel
    {
        Disco.Models.Repository.User User { get; set; }
        JobTableModel Jobs { get; set; }
        List<DocumentTemplate> DocumentTemplates { get; set; }
        List<DocumentTemplatePackage> DocumentTemplatePackages { get; set; }

        List<UserFlag> AvailableUserFlags { get; set; }
        
        IAuthorizationToken AuthorizationToken { get; set; }
        IClaimNavigatorItem ClaimNavigator { get; set; }
        DetailsResult UserDetails { get; set; }
        bool HasUserPhoto { get; set; }
        Dictionary<string, DetailsResult> AssignedDevicesDetails { get; set; }
    }
}