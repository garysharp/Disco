using Disco.Models.Services.Jobs.JobLists;
using Disco.Models.UI.Device;
using Disco.Services.Plugins;
using Disco.Web.Extensions;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Disco.Web.Models.Device
{
    public class ShowModel : DeviceShowModel
    {
        public Disco.Models.Repository.Device Device { get; set; }

        public List<Disco.Models.Repository.DeviceProfile> DeviceProfiles { get; set; }
        public Disco.Models.BI.Config.OrganisationAddress DeviceProfileDefaultOrganisationAddress { get; set; }
        public List<PluginFeatureManifest> DeviceProfileCertificateProviders { get; set; }
        public List<PluginFeatureManifest> DeviceProfileWirelessProfileProviders { get; set; }

        public List<Disco.Models.Repository.DeviceBatch> DeviceBatches { get; set; }
        public JobTableModel Jobs { get; set; }
        public List<Disco.Models.Repository.DeviceCertificate> Certificates { get; set; }

        public string OrganisationUnit { get; set; }

        public List<Disco.Models.Repository.DocumentTemplate> DocumentTemplates { get; set; }

        public List<SelectListItem> DocumentTemplatesSelectListItems
        {
            get
            {
                var list = new List<SelectListItem>();
                list.Add(new SelectListItem() { Selected = true, Value = string.Empty, Text = "Generate Document" });
                list.AddRange(this.DocumentTemplates.ToSelectListItems());
                return list;
            }
        }
    }
}