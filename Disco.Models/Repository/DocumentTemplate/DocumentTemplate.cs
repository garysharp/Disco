using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disco.Models.Repository
{
    public class DocumentTemplate
    {
        public const string PdfMimeType = "application/pdf";

        [StringLength(30), Required, Key]
        public string Id { get; set; }

        [StringLength(250), Required]
        public string Description { get; set; }
        [Required, StringLength(6)]
        public string Scope { get; set; }
        [DataType(DataType.MultilineText)]
        public string FilterExpression { get; set; }
        [DataType(DataType.MultilineText)]
        public string OnGenerateExpression { get; set; }
        [DataType(DataType.MultilineText)]
        public string OnImportAttachmentExpression { get; set; }
        public string OnImportUserFlagRules { get; set; }

        // Feature Request 2012-05-10 by G#: https://disco.uservoice.com/forums/159707-feedback/suggestions/2811092-document-template-option-flatten-form-on-generate
        public bool FlattenForm { get; set; }
        // End Feature Request

        public string DevicesLinkedGroup { get; set; }
        public string UsersLinkedGroup { get; set; }

        public bool IsHidden { get; set; }

        [InverseProperty("DocumentTemplates")]
        public virtual IList<JobSubType> JobSubTypes { get; set; }

        [NotMapped]
        public AttachmentTypes AttachmentType
        {
            get
            {
                switch (Scope)
                {
                    case DocumentTemplateScopes.Device:
                        return AttachmentTypes.Device;
                    case DocumentTemplateScopes.Job:
                        return AttachmentTypes.Job;
                    case DocumentTemplateScopes.User:
                        return AttachmentTypes.User;
                    default:
                        throw new ArgumentException("Unexpected Document Scope");
                }
            }
        }

        public static class DocumentTemplateScopes
        {
            public const string Device = "Device";
            public const string Job = "Job";
            public const string User = "User";

            public static List<string> ToList()
            {
                return new List<string> { Device, Job, User };
            }
        }

    }
}
