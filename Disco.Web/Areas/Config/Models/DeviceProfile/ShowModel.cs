using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Disco.Services.Plugins;
using Disco.Models.UI.Config.DeviceProfile;

namespace Disco.Web.Areas.Config.Models.DeviceProfile
{
    public class ShowModel : ConfigDeviceProfileShowModel
    {
        public Disco.Models.Repository.DeviceProfile DeviceProfile { get; set; }
        public List<SelectListItem> DeviceProfileDistributionTypes { get; set; }
        public List<Disco.Models.BI.Config.OrganisationAddress> OrganisationAddresses { get; set; }

        public List<PluginFeatureManifest> CertificateProviders { get; set; }

        public bool CanDelete { get; set; }
    }
}