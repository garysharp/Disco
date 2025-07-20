using Disco.Data.Repository;
using Disco.Models.UI.Config.DeviceBatch;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Web.Areas.Config.Models.DeviceBatch
{
    public class IndexModel : ConfigDeviceBatchIndexModel
    {
        public List<ConfigDeviceBatchIndexModelItem> DeviceBatches { get; set; }

        public static IndexModel Build(DiscoDataContext Database)
        {
            var m = new IndexModel();
            m.DeviceBatches = Database.DeviceBatches.OrderBy(db => db.Name).Select(db => new _IndexModelItem()
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
                InsuredUntil = db.InsuredUntil,
                IsLinked = db.AssignedUsersLinkedGroup != null || db.DevicesLinkedGroup != null
            }).ToArray().Cast<ConfigDeviceBatchIndexModelItem>().ToList();

            foreach (var item in m.DeviceBatches.Where(db => db.DefaultDeviceModel == null))
                item.DefaultDeviceModel = "<None Specified>";

            return m;
        }

    }
}