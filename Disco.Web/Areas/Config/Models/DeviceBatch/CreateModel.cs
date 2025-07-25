using Disco.Models.UI.Config.DeviceBatch;
using System;
using System.ComponentModel.DataAnnotations;

namespace Disco.Web.Areas.Config.Models.DeviceBatch
{
    public class CreateModel : ConfigDeviceBatchCreateModel
    {
        [Required, StringLength(500)]
        public string Name { get; set; }

        [Required, DisplayFormat(ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true, DataFormatString = "{0:yyyy/MM/dd}", HtmlEncode = false)]
        public DateTime PurchaseDate { get; set; }
    }
}