using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Disco.Data.Repository;
using Disco.BI.Extensions;
using Disco.Models.UI.Config.DeviceProfile;

namespace Disco.Web.Areas.Config.Models.DeviceProfile
{
    public class IndexModel : ConfigDeviceProfileIndexModel
    {
        public List<ConfigDeviceProfileIndexModelItem> DeviceProfiles { get; set; }

        public static IndexModel Build(DiscoDataContext dbContext)
        {
            var m = new IndexModel();
            m.DeviceProfiles = dbContext.DeviceProfiles.OrderBy(dp => dp.Name).Select(dp => new _IndexModelItem()
            {
                Id = dp.Id,
                Name = dp.Name,
                ShortName = dp.ShortName,
                Address = dp.DefaultOrganisationAddress,
                Description = dp.Description,
                DistributionTypeId = dp.DistributionTypeDb,
                DeviceCount = dp.Devices.Count,
                DeviceDecommissionedCount = dp.Devices.Count(d => d.DecommissionedDate.HasValue)
            }).Cast<ConfigDeviceProfileIndexModelItem>().ToList();

            if (DiscoApplication.MultiSiteMode)
            {
                foreach (var dp in m.DeviceProfiles)
                    if (dp.Address.HasValue)
                        dp.AddressName = dbContext.DiscoConfiguration.OrganisationAddresses.GetAddress(dp.Address.Value).Name;
            }

            return m;
        }

    }
}