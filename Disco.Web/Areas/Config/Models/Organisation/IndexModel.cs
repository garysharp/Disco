using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Disco.Models.BI.Config;
using System.ComponentModel.DataAnnotations;

namespace Disco.Web.Areas.Config.Models.Organisation
{
    public class IndexModel
    {
        public string OrganisationName { get; set; }
        [Display(Name="Enabled")]
        public bool MultiSiteMode { get; set; }
        public List<OrganisationAddress> OrganisationAddresses { get; set; }
    }
}