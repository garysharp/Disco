using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disco.Models.Repository
{
    public class DeviceAttachment
    {
        [Key]
        public int Id { get; set; }
        public string DeviceSerialNumber { get; set; }
        [Required]
        public string TechUserId { get; set; }
        [StringLength(500), Required]
        public string Filename { get; set; }
        [Required, StringLength(500)]
        public string MimeType { get; set; }
        public DateTime Timestamp { get; set; }
        [Required, StringLength(500)]
        public string Comments { get; set; }

        public string DocumentTemplateId { get; set; }

        [InverseProperty("DeviceAttachments"), ForeignKey("DeviceSerialNumber")]
        public virtual Device Device { get; set; }

        [ForeignKey("TechUserId")]
        public virtual User TechUser { get; set; }

        [ForeignKey("DocumentTemplateId")]
        public virtual DocumentTemplate DocumentTemplate { get; set; }

    }
}
