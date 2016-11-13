using Disco.Models.Services.Documents;
using System.Collections.Generic;

namespace Disco.Models.UI.Config.DocumentTemplate
{
    public interface ConfigDocumentTemplateCreatePackageModel : BaseUIModel
    {
        DocumentTemplatePackage Package { get; set; }

        List<string> Scopes { get; }
    }
}
