using Disco.Models.Repository;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Disco.Models.Services.Documents
{
    public class DocumentTemplatePackage
    {
        [Key, StringLength(30), Required]
        public string Id { get; set; }
        [StringLength(250), Required]
        public string Description { get; set; }
        [Required]
        public AttachmentTypes Scope { get; set; }
        public List<string> JobSubTypes { get; set; }

        public List<string> DocumentTemplateIds { get; set; }

        [DataType(DataType.MultilineText)]
        public string FilterExpression { get; set; }
        [DataType(DataType.MultilineText)]
        public string OnGenerateExpression { get; set; }

        public bool IsHidden { get; set; }

        /// <summary>
        /// Indicates blank pages should be added so that documents will be separated when duplex printed.
        /// </summary>
        public bool InsertBlankPages { get; set; }
    }
}
