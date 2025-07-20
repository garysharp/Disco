using System.Collections.Generic;

namespace Disco.Models.UI.Config.DocumentTemplate
{
    public interface ConfigDocumentTemplateUndetectedPagesModel : BaseUIModel
    {
        List<Repository.DocumentTemplate> DocumentTemplates { get; set; }
    }
}
