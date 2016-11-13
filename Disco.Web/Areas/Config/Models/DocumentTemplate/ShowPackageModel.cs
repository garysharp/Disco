using Disco.Models.Repository;
using Disco.Models.Services.Documents;
using Disco.Models.UI.Config.DocumentTemplate;
using System.Collections.Generic;

namespace Disco.Web.Areas.Config.Models.DocumentTemplate
{
    public class ShowPackageModel : ConfigDocumentTemplateShowPackageModel
    {
        public DocumentTemplatePackage Package { get; set; }
        public List<JobSubType> JobSubTypesSelected { get; set; }
        public List<Disco.Models.Repository.DocumentTemplate> DocumentTemplates { get; set; }
        public List<Disco.Models.Repository.DocumentTemplate> DocumentTemplatesSelected { get; set; }
        public List<JobType> JobTypes { get; set; }
        public List<string> Scopes
        {
            get
            {
                return Disco.Models.Repository.DocumentTemplate.DocumentTemplateScopes.ToList();
            }
        }

    }
}