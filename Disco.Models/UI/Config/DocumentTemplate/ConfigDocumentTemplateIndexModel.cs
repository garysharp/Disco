using System.Collections.Generic;

namespace Disco.Models.UI.Config.DocumentTemplate
{
    public interface ConfigDocumentTemplateIndexModel : BaseUIModel
    {
        Dictionary<Repository.DocumentTemplate, int> DocumentTemplates { get; set; }
    }
}
