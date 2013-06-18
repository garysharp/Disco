using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.UI.Device
{
    public interface DeviceShowModel : BaseUIModel
    {
        Disco.Models.Repository.Device Device { get; set; }

        List<Disco.Models.Repository.DeviceProfile> DeviceProfiles { get; set; }
        Disco.Models.BI.Config.OrganisationAddress DeviceProfileDefaultOrganisationAddress { get; set; }

        List<Disco.Models.Repository.DeviceBatch> DeviceBatches { get; set; }

        Disco.Models.BI.Job.JobTableModel Jobs { get; set; }

        List<Disco.Models.Repository.DeviceCertificate> Certificates { get; set; }

        List<Disco.Models.Repository.DocumentTemplate> DocumentTemplates { get; set; }
    }
}
