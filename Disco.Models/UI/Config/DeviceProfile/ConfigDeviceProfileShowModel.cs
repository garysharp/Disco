using Disco.Models.BI.Config;
using Disco.Models.UI.Config.Shared;
using System.Collections.Generic;

namespace Disco.Models.UI.Config.DeviceProfile
{
    public interface ConfigDeviceProfileShowModel : BaseUIModel, ConfigSharedDeviceGroupDocumentTemplateBulkGenerate
    {
        Repository.DeviceProfile DeviceProfile { get; set; }
        OrganisationAddress DefaultOrganisationAddress { get; set; }
        List<OrganisationAddress> OrganisationAddresses { get; set; }

        string FriendlyOrganisationalUnitName { get; }
        bool OrganisationalUnitExists { get; set; }

        int DeviceCount { get; set; }
        int DeviceDecommissionedCount { get; set; }

        bool CanDelete { get; set; }
        bool CanDecommission { get; set; }
    }
}
