using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disco.Models.Repository
{
    public class DeviceAttachment : IAttachment
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
        [StringLength(500)]
        public string Comments { get; set; }

        public string DocumentTemplateId { get; set; }

        [StringLength(30)]
        public string HandlerId { get; set; }
        [StringLength(50)]
        public string HandlerReferenceId { get; set; }
        public string HandlerData { get; set; }

        [NotMapped]
        public object Reference { get { return DeviceSerialNumber; } }

        [NotMapped]
        public AttachmentTypes AttachmentType { get { return AttachmentTypes.Device; } }

        [InverseProperty("DeviceAttachments"), ForeignKey("DeviceSerialNumber")]
        public virtual Device Device { get; set; }

        [ForeignKey("TechUserId")]
        public virtual User TechUser { get; set; }

        [ForeignKey("DocumentTemplateId")]
        public virtual DocumentTemplate DocumentTemplate { get; set; }
    }
}
