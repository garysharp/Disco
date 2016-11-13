using Disco.Models.Services.Documents;
using Disco.Models.UI.Config.DocumentTemplate;
using System.Collections.Generic;

namespace Disco.Web.Areas.Config.Models.DocumentTemplate
{
    public class IndexModel : ConfigDocumentTemplateIndexModel
    {
        public Dictionary<Disco.Models.Repository.DocumentTemplate, int> DocumentTemplates { get; set; }

        public List<DocumentTemplatePackage> Packages { get; set; }
    }
}