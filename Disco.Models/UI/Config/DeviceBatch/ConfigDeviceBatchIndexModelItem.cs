using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.UI.Config.DeviceBatch
{
    public interface ConfigDeviceBatchIndexModelItem
    {
        int Id { get; set; }
        string Name { get; set; }
        [Required(), DisplayFormat(ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true, DataFormatString = "{0:yyyy/MM/dd}", HtmlEncode = false)]
        DateTime PurchaseDate { get; set; }
        int DeviceCount { get; set; }
        int DeviceDecommissionedCount { get; set; }
        int? PurchaseUnitQuantity { get; set; }
        string DefaultDeviceModel { get; set; }
        DateTime? WarrantyExpires { get; set; }
        DateTime? InsuredUntil { get; set; }
        string InsuranceSupplier { get; set; }
    }
}
