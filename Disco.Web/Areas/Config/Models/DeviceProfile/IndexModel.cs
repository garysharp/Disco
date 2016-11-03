using Disco.Data.Repository;
using Disco.Models.UI.Config.DeviceProfile;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Web.Areas.Config.Models.DeviceProfile
{
    public class IndexModel : ConfigDeviceProfileIndexModel
    {
        public List<ConfigDeviceProfileIndexModelItem> DeviceProfiles { get; set; }

        public static IndexModel Build(DiscoDataContext Database)
        {
            var m = new IndexModel();
            m.DeviceProfiles = Database.DeviceProfiles.OrderBy(dp => dp.Name).Select(dp => new _IndexModelItem()
            {
                Id = dp.Id,
                Name = dp.Name,
                ShortName = dp.ShortName,
                Address = dp.DefaultOrganisationAddress,
                Description = dp.Description,
                DistributionType = dp.DistributionType.Value,
                DeviceCount = dp.Devices.Count,
                DeviceDecommissionedCount = dp.Devices.Count(d => d.DecommissionedDate.HasValue)
            }).ToArray().Cast<ConfigDeviceProfileIndexModelItem>().ToList();

            if (DiscoApplication.MultiSiteMode)
            {
                foreach (var dp in m.DeviceProfiles)
                {
                    if (dp.Address.HasValue)
                    {
                        dp.AddressName = Database.DiscoConfiguration.OrganisationAddresses.GetAddress(dp.Address.Value)?.Name;
                    }
                }
            }

            return m;
        }

    }
}