using Disco.Models.BI.Config;
using Disco.Models.Services.Jobs.JobLists;
using System.Collections.Generic;

namespace Disco.Models.UI.Device
{
    public interface DeviceShowModel : BaseUIModel
    {
        Repository.Device Device { get; set; }

        List<Repository.DeviceProfile> DeviceProfiles { get; set; }
        OrganisationAddress DeviceProfileDefaultOrganisationAddress { get; set; }

        List<Repository.DeviceBatch> DeviceBatches { get; set; }

        JobTableModel Jobs { get; set; }

        List<Repository.DeviceCertificate> Certificates { get; set; }

        List<Repository.DocumentTemplate> DocumentTemplates { get; set; }
    }
}
