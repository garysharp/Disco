using Disco.Models.Repository;
using Disco.Models.UI.Config.DocumentTemplate;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Disco.Web.Areas.Config.Models.DocumentTemplate
{
    public class CreatePackageModel : ConfigDocumentTemplateCreatePackageModel
    {
        [StringLength(30), Required]
        public string Id { get; set; }
        [StringLength(250), Required]
        public string Description { get; set; }
        [Required]
        public AttachmentTypes Scope { get; set; }

        public List<string> Scopes
            => Disco.Models.Repository.DocumentTemplate.DocumentTemplateScopes.ToList();

    }

}