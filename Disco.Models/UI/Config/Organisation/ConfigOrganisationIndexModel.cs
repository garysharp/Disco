using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.UI.Config.Organisation
{
    public interface ConfigOrganisationIndexModel : BaseUIModel
    {
        string OrganisationName { get; set; }
        bool MultiSiteMode { get; set; }
        List<Models.BI.Config.OrganisationAddress> OrganisationAddresses { get; set; }
    }
}
