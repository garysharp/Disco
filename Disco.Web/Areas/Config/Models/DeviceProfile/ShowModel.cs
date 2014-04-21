using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Disco.Services.Plugins;
using Disco.Models.UI.Config.DeviceProfile;
using Disco.Services.Interop.ActiveDirectory;

namespace Disco.Web.Areas.Config.Models.DeviceProfile
{
    public class ShowModel : ConfigDeviceProfileShowModel
    {
        public Disco.Models.Repository.DeviceProfile DeviceProfile { get; set; }
        public List<SelectListItem> DeviceProfileDistributionTypes { get; set; }
        public Disco.Models.BI.Config.OrganisationAddress DefaultOrganisationAddress { get; set; }
        public List<Disco.Models.BI.Config.OrganisationAddress> OrganisationAddresses { get; set; }

        public string FriendlyOrganisationalUnitName
        {
            get
            {
                if (string.IsNullOrEmpty(this.DeviceProfile.OrganisationalUnit))
                {
                    var domain = ActiveDirectory.Context.PrimaryDomain;
                    return domain.FriendlyDistinguishedNamePath(domain.DefaultComputerContainer);
                }
                else
                {
                    var domain = ActiveDirectory.Context.GetDomainFromDistinguishedName(this.DeviceProfile.OrganisationalUnit);
                    return domain.FriendlyDistinguishedNamePath(this.DeviceProfile.OrganisationalUnit);
                }
            }
        }

        public List<PluginFeatureManifest> CertificateProviders { get; set; }

        public int DeviceCount { get; set; }
        public int DeviceDecommissionedCount { get; set; }

        public bool CanDelete { get; set; }
    }
}