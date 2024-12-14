using System.Collections.Generic;
using Disco.Models.BI.Config;
using System.ComponentModel.DataAnnotations;
using Disco.Models.UI.Config.Organisation;

namespace Disco.Web.Areas.Config.Models.Organisation
{
    public class IndexModel : ConfigOrganisationIndexModel
    {
        public string OrganisationName { get; set; }
        [Display(Name="Enabled")]
        public bool MultiSiteMode { get; set; }
        public List<OrganisationAddress> OrganisationAddresses { get; set; }
    }
}