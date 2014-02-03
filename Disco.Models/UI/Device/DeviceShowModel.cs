using Disco.Models.Services.Jobs.JobLists;
using System.Collections.Generic;

namespace Disco.Models.UI.Device
{
    public interface DeviceShowModel : BaseUIModel
    {
        Disco.Models.Repository.Device Device { get; set; }

        List<Disco.Models.Repository.DeviceProfile> DeviceProfiles { get; set; }
        Disco.Models.BI.Config.OrganisationAddress DeviceProfileDefaultOrganisationAddress { get; set; }

        List<Disco.Models.Repository.DeviceBatch> DeviceBatches { get; set; }

        JobTableModel Jobs { get; set; }

        List<Disco.Models.Repository.DeviceCertificate> Certificates { get; set; }

        List<Disco.Models.Repository.DocumentTemplate> DocumentTemplates { get; set; }
    }
}
