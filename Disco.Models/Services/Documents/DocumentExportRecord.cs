using Disco.Models.Exporting;
using Disco.Models.Repository;
using System.Collections.Generic;

namespace Disco.Models.Services.Documents
{
    public class DocumentExportRecord : IExportRecord
    {
        public DocumentTemplate DocumentTemplate { get; set; }
        public IAttachment Attachment { get; set; }
        public IAttachmentTarget AttachmentTarget { get; set; }

        public Device Device { get; set; }

        public Job Job { get; set; }
        public string JobStatus { get; set; }
        public string JobTypeDescription { get; set; }
        public IEnumerable<string> JobSubTypeDescriptions { get; set; }

        public User User { get; set; }
        public Dictionary<string, string> UserCustomDetails { get; set; }
    }
}
