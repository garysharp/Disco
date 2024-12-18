using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disco.Models.Repository
{
    public class UserAttachment : IAttachment
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        [Required]
        public string TechUserId { get; set; }
        [Required, StringLength(500)]
        public string Filename { get; set; }
        [StringLength(500), Required]
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
        public object Reference { get { return UserId; } }

        [NotMapped]
        public AttachmentTypes AttachmentType { get { return AttachmentTypes.User; } }

        [ForeignKey("UserId"), InverseProperty("UserAttachments")]
        public virtual User User { get; set; }

        [ForeignKey("TechUserId")]
        public virtual User TechUser { get; set; }

        [ForeignKey("DocumentTemplateId")]
        public virtual DocumentTemplate DocumentTemplate { get; set; }
    }
}
