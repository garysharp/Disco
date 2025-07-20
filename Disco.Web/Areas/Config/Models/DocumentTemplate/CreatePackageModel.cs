using Disco.Models.Services.Documents;
using Disco.Models.UI.Config.DocumentTemplate;
using System.Collections.Generic;

namespace Disco.Web.Areas.Config.Models.DocumentTemplate
{
    public class CreatePackageModel : ConfigDocumentTemplateCreatePackageModel
    {
        public DocumentTemplatePackage Package { get; set; }

        public List<string> Scopes
        {
            get
            {
                return Disco.Models.Repository.DocumentTemplate.DocumentTemplateScopes.ToList();
            }
        }

    }

}