using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disco.Models.Repository
{
    public class JobMetaWarranty
    {
        [Required, Key]
        public int JobId { get; set; }

        [StringLength(100)]
        public string ExternalName { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true, DataFormatString = "{0:yyyy/MM/dd hh:mm tt}", HtmlEncode = false)]
        public DateTime? ExternalLoggedDate { get; set; }
        [StringLength(100)]
        public string ExternalReference { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true, DataFormatString = "{0:yyyy/MM/dd hh:mm tt}", HtmlEncode = false)]
        public DateTime? ExternalCompletedDate { get; set; }

        [ForeignKey("JobId"), Required]
        public virtual Job Job { get; set; }
    }
}
