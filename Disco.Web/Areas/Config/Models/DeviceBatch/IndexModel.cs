using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Disco.Data.Repository;

namespace Disco.Web.Areas.Config.Models.DeviceBatch
{
    public class IndexModel
    {
        public List<_IndexModelDeviceBatch> DeviceBatches { get; set; }

        public static IndexModel Build(DiscoDataContext dbContext)
        {
            var m = new IndexModel();
            m.DeviceBatches = dbContext.DeviceBatches.OrderBy(db => db.Name).Select(db => new _IndexModelDeviceBatch()
            {
                Id = db.Id,
                Name = db.Name,
                PurchaseDate = db.PurchaseDate,
                PurchaseUnitQuantity = db.UnitQuantity,
                DeviceCount = db.Devices.Count,
                DeviceDecommissionedCount = db.Devices.Count(d=> d.DecommissionedDate.HasValue),
                DefaultDeviceModel = db.DefaultDeviceModel.Description,
                WarrantyExpires = db.WarrantyValidUntil,
                InsuranceSupplier = db.InsuranceSupplier,
                InsuredUntil = db.InsuredUntil
            }).ToList();

            return m;
        }

    }
}