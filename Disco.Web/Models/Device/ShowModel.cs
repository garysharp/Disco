using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Disco.BI;
using Disco.BI.Extensions;
using Disco.Models.UI.Device;
using Disco.Web.Extensions;
using Disco.Services.Plugins;

namespace Disco.Web.Models.Device
{
    public class ShowModel : DeviceShowModel
    {
        public Disco.Models.Repository.Device Device { get; set; }

        public List<Disco.Models.Repository.DeviceProfile> DeviceProfiles { get; set; }
        public Disco.Models.BI.Config.OrganisationAddress DeviceProfileDefaultOrganisationAddress { get; set; }
        public PluginFeatureManifest DeviceProfileCertificateProvider { get; set; }

        public List<Disco.Models.Repository.DeviceBatch> DeviceBatches { get; set; }
        public Disco.Models.BI.Job.JobTableModel Jobs { get; set; }
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