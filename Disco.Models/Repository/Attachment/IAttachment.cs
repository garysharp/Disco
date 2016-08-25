using System;

namespace Disco.Models.Repository
{
    public interface IAttachment
    {
        int Id { get; set; }

        object Reference { get; }

        string TechUserId { get; set; }

        string Filename { get; set; }
        string MimeType { get; set; }

        DateTime Timestamp { get; set; }

        string Comments { get; set; }

        string DocumentTemplateId { get; set; }

        AttachmentTypes AttachmentType { get; }
    }
}
