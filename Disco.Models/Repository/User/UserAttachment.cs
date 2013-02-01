using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disco.Models.Repository
{
    public class UserAttachment
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
        [Required, StringLength(500)]
        public string Comments { get; set; }

        public string DocumentTemplateId { get; set; }

        [ForeignKey("UserId"), InverseProperty("UserAttachments")]
        public virtual User User { get; set; }

        [ForeignKey("TechUserId")]
        public virtual User TechUser { get; set; }

        [ForeignKey("DocumentTemplateId")]
        public virtual DocumentTemplate DocumentTemplate { get; set; }
    }
}
