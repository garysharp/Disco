using System.Collections.Generic;
using System.Linq;
using Disco.Data.Repository;
using Disco.Models.UI.Config.DeviceModel;

namespace Disco.Web.Areas.Config.Models.DeviceModel
{
    public class IndexModel : ConfigDeviceModelIndexModel
    {
        public List<ConfigDeviceModelIndexModelItem> DeviceModels { get; set; }

        public static IndexModel Build(DiscoDataContext Database)
        {
            var m = new IndexModel();
            m.DeviceModels = Database.DeviceModels.OrderBy(dm => dm.Description).Select(dm => new _IndexModelItem()
            {
                Id = dm.Id,
                Name = dm.Description,
                Manufacturer = dm.Manufacturer,
                Model = dm.Model,
                ModelType = dm.ModelType,
                DeviceCount = dm.Devices.Count,
                DeviceDecommissionedCount = dm.Devices.Count(d => d.DecommissionedDate.HasValue)
            }).ToArray().Cast<ConfigDeviceModelIndexModelItem>().ToList();

            return m;
        }
    }
}