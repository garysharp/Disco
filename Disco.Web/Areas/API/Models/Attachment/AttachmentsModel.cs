using System.Collections.Generic;

namespace Disco.Web.Areas.API.Models.Attachment
{
    public class AttachmentsModel
    {
        public List<_AttachmentModel> Attachments { get; set; }
        public string Result { get; set; }
    }
}