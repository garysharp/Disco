using Disco.Models.BI.Config;
using System.Collections.Generic;

namespace Disco.Models.UI.Config.DeviceProfile
{
    public interface ConfigDeviceProfileShowModel : BaseUIModel
    {
        Repository.DeviceProfile DeviceProfile { get; set; }
        OrganisationAddress DefaultOrganisationAddress { get; set; }

        List<OrganisationAddress> OrganisationAddresses { get; set; }

        int DeviceCount { get; set; }
        int DeviceDecommissionedCount { get; set; }

        bool CanDelete { get; set; }
    }
}
