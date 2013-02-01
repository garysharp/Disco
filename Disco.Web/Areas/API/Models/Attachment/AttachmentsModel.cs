using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disco.Web.Areas.API.Models.Attachment
{
    public class AttachmentsModel
    {
        public List<_AttachmentModel> Attachments { get; set; }
        public string Result { get; set; }
    }
}