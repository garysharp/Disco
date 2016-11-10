using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Disco.Models.UI.Config.DeviceBatch;

namespace Disco.Web.Areas.Config.Models.DeviceBatch
{
    public class _IndexModelItem : ConfigDeviceBatchIndexModelItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [Required(), DisplayFormat(ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true, DataFormatString = "{0:yyyy/MM/dd}", HtmlEncode = false)]
        public DateTime PurchaseDate { get; set; }
        public int DeviceCount { get; set; }
        public int DeviceDecommissionedCount { get; set; }
        public int? PurchaseUnitQuantity { get; set; }
        public string DefaultDeviceModel { get; set; }
        public DateTime? WarrantyExpires { get; set; }
        public DateTime? InsuredUntil { get; set; }
        public string InsuranceSupplier { get; set; }
        public bool IsLinked { get; set; }
    }
}