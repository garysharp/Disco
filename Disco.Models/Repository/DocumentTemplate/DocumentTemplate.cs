using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        [StringLength(250)]
        public string FilterExpression { get; set; }

        // Feature Request 2012-05-10 by G#: https://disco.uservoice.com/forums/159707-feedback/suggestions/2811092-document-template-option-flatten-form-on-generate
        public bool FlattenForm { get; set; }
        // End Feature Request

        [InverseProperty("DocumentTemplates")]
        public virtual IList<JobSubType> JobSubTypes { get; set; }

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
