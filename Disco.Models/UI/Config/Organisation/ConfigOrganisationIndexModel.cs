using Disco.Models.BI.Config;
using System.Collections.Generic;

namespace Disco.Models.UI.Config.Organisation
{
    public interface ConfigOrganisationIndexModel : BaseUIModel
    {
        string OrganisationName { get; set; }
        bool MultiSiteMode { get; set; }
        List<OrganisationAddress> OrganisationAddresses { get; set; }
    }
}
