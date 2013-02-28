using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Disco.Services.Plugins;

namespace Disco.Web.Areas.Config.Models.DeviceProfile
{
    public class ShowModel
    {
        public Disco.Models.Repository.DeviceProfile DeviceProfile { get; set; }
        public List<SelectListItem> DeviceProfileDistributionTypes { get; set; }
        public List<Disco.Models.BI.Config.OrganisationAddress> OrganisationAddresses { get; set; }

        public List<PluginFeatureManifest> CertificateProviders { get; set; }

        public bool CanDelete { get; set; }
    }
}