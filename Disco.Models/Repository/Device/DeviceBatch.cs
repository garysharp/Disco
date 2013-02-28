using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disco.Models.Repository
{
    public class DeviceBatch
    {
        [Key]
        public int Id { get; set; }
        [StringLength(500)]
        public string Name { get; set; }

        [Required, DisplayFormat(ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true, DataFormatString = "{0:yyyy/MM/dd}", HtmlEncode = false)]
        public DateTime PurchaseDate { get; set; }
        [StringLength(200)]
        public string Supplier { get; set; }
        [DataType(DataType.MultilineText), StringLength(500)]
        public string PurchaseDetails { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true, DataFormatString = "{0:c}")]
        public decimal? UnitCost { get; set; }
        public int? UnitQuantity { get; set; }

        public int? DefaultDeviceModelId { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true, DataFormatString = "{0:yyyy/MM/dd}", HtmlEncode = false)]
        public DateTime? WarrantyValidUntil { get; set; }
        [DataType(DataType.MultilineText)]
        public string WarrantyDetails { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true, DataFormatString = "{0:yyyy/MM/dd}", HtmlEncode = false)]
        public DateTime? InsuredDate { get; set; }
        [StringLength(200)]
        public string InsuranceSupplier { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true, DataFormatString = "{0:yyyy/MM/dd}", HtmlEncode = false)]
        public DateTime? InsuredUntil { get; set; }
        [DataType(DataType.MultilineText)]
        public string InsuranceDetails { get; set; }

        [DataType(DataType.MultilineText)]
        public string Comments { get; set; }

        [ForeignKey("DefaultDeviceModelId")]
        public virtual DeviceModel DefaultDeviceModel { get; set; }

        public virtual IList<Device> Devices { get; set; }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(this.Name))
            {
                return string.Format("{0}: {1}", this.Id, this.PurchaseDate.ToLongDateString());
            }
            return this.Name;
        }
    }
}
