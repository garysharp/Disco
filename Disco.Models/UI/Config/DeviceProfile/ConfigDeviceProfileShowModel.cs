using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.UI.Config.DeviceProfile
{
    public interface ConfigDeviceProfileShowModel : BaseUIModel
    {
        Disco.Models.Repository.DeviceProfile DeviceProfile { get; set; }
        Disco.Models.BI.Config.OrganisationAddress DefaultOrganisationAddress { get; set; }

        List<Disco.Models.BI.Config.OrganisationAddress> OrganisationAddresses { get; set; }

        int DeviceCount { get; set; }
        int DeviceDecommissionedCount { get; set; }

        bool CanDelete { get; set; }
    }
}
