using Disco.Models.Repository;
using Disco.Models.Services.Documents;
using System.Collections.Generic;

namespace Disco.Models.UI.Config.DocumentTemplate
{
    public interface ConfigDocumentTemplateShowPackageModel : BaseUIModel
    {
        DocumentTemplatePackage Package { get; set; }
        List<JobSubType> JobSubTypesSelected { get; set; }
        List<Repository.DocumentTemplate> DocumentTemplates { get; set; }
        List<Repository.DocumentTemplate> DocumentTemplatesSelected { get; set; }

        List<JobType> JobTypes { get; set; }

        List<string> Scopes { get; }
    }
}
