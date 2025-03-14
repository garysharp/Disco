using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disco.Models.Repository
{
    public class DeviceBatchAttachment : IAttachment
    {
        [Key]
        public int Id { get; set; }
        public int DeviceBatchId { get; set; }
        [Required]
        public string TechUserId { get; set; }
        [StringLength(500), Required]
        public string Filename { get; set; }
        [Required, StringLength(500)]
        public string MimeType { get; set; }
        public DateTime Timestamp { get; set; }
        [StringLength(500)]
        public string Comments { get; set; }

        [NotMapped]
        public string HandlerId { get => null; set { } }
        [NotMapped]
        public string HandlerReferenceId { get => null; set { } }
        [NotMapped]
        public string HandlerData { get => null; set { } }

        [NotMapped]
        public object Reference => DeviceBatchId;

        [NotMapped]
        public AttachmentTypes AttachmentType => AttachmentTypes.DeviceBatch;

        [NotMapped]
        public string DocumentTemplateId { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }


        [InverseProperty(nameof(Repository.DeviceBatch.DeviceBatchAttachments)), ForeignKey(nameof(DeviceBatchId))]
        public virtual DeviceBatch DeviceBatch { get; set; }

        [ForeignKey("TechUserId")]
        public virtual User TechUser { get; set; }
    }
}
