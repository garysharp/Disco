using Disco.Models.BI.Config;
using Disco.Models.UI.Config.Organisation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Disco.Web.Areas.Config.Models.Organisation
{
    public class IndexModel : ConfigOrganisationIndexModel
    {
        public string OrganisationName { get; set; }
        [Display(Name = "Enabled")]
        public bool MultiSiteMode { get; set; }
        public List<OrganisationAddress> OrganisationAddresses { get; set; }
    }
}