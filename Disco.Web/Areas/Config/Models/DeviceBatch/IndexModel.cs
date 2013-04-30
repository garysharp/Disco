using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Disco.Data.Repository;
using Disco.Models.UI.Config.DeviceBatch;

namespace Disco.Web.Areas.Config.Models.DeviceBatch
{
    public class IndexModel : ConfigDeviceBatchIndexModel
    {
        public List<ConfigDeviceBatchIndexModelItem> DeviceBatches { get; set; }

        public static IndexModel Build(DiscoDataContext dbContext)
        {
            var m = new IndexModel();
            m.DeviceBatches = dbContext.DeviceBatches.OrderBy(db => db.Name).Select(db => new _IndexModelItem()
            {
                Id = db.Id,
                Name = db.Name,
                PurchaseDate = db.PurchaseDate,
                PurchaseUnitQuantity = db.UnitQuantity,
                DeviceCount = db.Devices.Count,
                DeviceDecommissionedCount = db.Devices.Count(d => d.DecommissionedDate.HasValue),
                DefaultDeviceModel = db.DefaultDeviceModel.Description,
                WarrantyExpires = db.WarrantyValidUntil,
                InsuranceSupplier = db.InsuranceSupplier,
                InsuredUntil = db.InsuredUntil
            }).Cast<ConfigDeviceBatchIndexModelItem>().ToList();

            return m;
        }

    }
}