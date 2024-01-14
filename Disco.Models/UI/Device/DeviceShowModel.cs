﻿using Disco.Models.BI.Config;
using Disco.Models.Repository;
using Disco.Models.Services.Documents;
using Disco.Models.Services.Jobs.JobLists;
using System.Collections.Generic;

namespace Disco.Models.UI.Device
{
    public interface DeviceShowModel : BaseUIModel
    {
        Repository.Device Device { get; set; }

        List<Repository.DeviceProfile> DeviceProfiles { get; set; }
        HashSet<int> DecommissionedDeviceProfileIds { get; set; }
        OrganisationAddress DeviceProfileDefaultOrganisationAddress { get; set; }

        List<Repository.DeviceBatch> DeviceBatches { get; set; }
        HashSet<int> DecommissionedDeviceBatchIds { get; set; }

        JobTableModel Jobs { get; set; }

        List<Repository.DeviceCertificate> Certificates { get; set; }

        List<Repository.DocumentTemplate> DocumentTemplates { get; set; }
        List<DocumentTemplatePackage> DocumentTemplatePackages { get; set; }

        List<DeviceFlag> AvailableDeviceFlags { get; set; }

        Dictionary<string, string> AssignedUserDetails { get; set; }
        bool HasAssignedUserPhoto { get; set; }
    }
}
