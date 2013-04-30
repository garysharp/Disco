using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.UI.Config.DeviceProfile
{
    public interface ConfigDeviceProfileIndexModelItem
    {
        int Id { get; set; }
        string Name { get; set; }
        string ShortName { get; set; }
        int? Address { get; set; }
        string AddressName { get; set; }
        string Description { get; set; }
        int DistributionTypeId { get; set; }

        string DistributionType { get; }

        int DeviceCount { get; set; }
        int DeviceDecommissionedCount { get; set; }
    }
}
