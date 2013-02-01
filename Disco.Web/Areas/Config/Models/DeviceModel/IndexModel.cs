using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Disco.Data.Repository;

namespace Disco.Web.Areas.Config.Models.DeviceModel
{
    public class IndexModel
    {
        public List<_IndexModelDeviceModel> DeviceModels { get; set; }

        public static IndexModel Build(DiscoDataContext dbContext)
        {
            var m = new IndexModel();
            m.DeviceModels = dbContext.DeviceModels.OrderBy(dm => dm.Description).Select(dm => new _IndexModelDeviceModel()
            {
                Id = dm.Id,
                Name = dm.Description,
                Manufacturer = dm.Manufacturer,
                Model = dm.Model,
                ModelType = dm.ModelType,
                DeviceCount = dm.Devices.Count
            }).ToList();

            return m;
        }

    }
}