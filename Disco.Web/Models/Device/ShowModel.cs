using Disco.Models.Services.Documents;
using Disco.Models.Services.Jobs.JobLists;
using Disco.Models.Services.Plugins.Details;
using Disco.Models.UI.Device;
using Disco.Services.Plugins;
using Disco.Web.Models.Shared;
using System.Collections.Generic;

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
        public List<DocumentTemplatePackage> DocumentTemplatePackages { get; set; }
        public GenerateDocumentControlModel GenerateDocumentControlModel => new GenerateDocumentControlModel()
        {
            Target = Device,
            Templates = DocumentTemplates,
            TemplatePackages = DocumentTemplatePackages,
        };

        public DetailsResult DeviceDetails { get; set; }
        public DetailsResult AssignedUserDetails { get; set; }
        public bool HasAssignedUserPhoto { get; set; }
    }
}