using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Disco.Models.Repository
{
    public class DeviceModel
    {
        public static readonly string CustomModelType = "Custom";

        [Key]
        public int Id { get; set; }
        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(200)]
        public string Manufacturer { get; set; }
        [StringLength(200)]
        public string Model { get; set; }
        [StringLength(40)]
        public string ModelType { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true, DataFormatString = "{0:yyyy/MM/dd}", HtmlEncode = false)]
        public DateTime? DefaultPurchaseDate { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true, DataFormatString = "{0:c}")]
        public decimal? DeviceCost { get; set; }

        [StringLength(40)]
        public string DefaultWarrantyProvider { get; set; }

        [StringLength(40)]
        public string DefaultRepairProvider { get; set; }

        public virtual IList<DeviceComponent> DeviceComponents { get; set; }

        public virtual IList<Device> Devices { get; set; }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Description))
            {
                return $"{Manufacturer} {Model}";
            }
            return Description;
        }

        public bool IsCustomModel()
            => CustomModelType.Equals(ModelType, StringComparison.Ordinal);
    }
}
