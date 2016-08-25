using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disco.Models.Repository
{
    public class JobAttachment : IAttachment
    {
        [Key]
        public int Id { get; set; }
        public int JobId { get; set; }

        [Required]
        public string TechUserId { get; set; }
        [Required, StringLength(500)]
        public string Filename { get; set; }
        [Required, StringLength(500)]
        public string MimeType { get; set; }
        public DateTime Timestamp { get; set; }
        [StringLength(500), Required]
        public string Comments { get; set; }

        public string DocumentTemplateId { get; set; }

        [NotMapped]
        public object Reference { get { return JobId; } }

        [NotMapped]
        public AttachmentTypes AttachmentType { get { return AttachmentTypes.Job; } }

        [ForeignKey("JobId"), InverseProperty("JobAttachments")]
        public virtual Job Job { get; set; }

        [ForeignKey("TechUserId")]
        public virtual User TechUser { get; set; }

        [ForeignKey("DocumentTemplateId")]
        public virtual DocumentTemplate DocumentTemplate { get; set; } 
    }
}
