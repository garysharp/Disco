using System.Collections.Generic;

namespace Disco.Models.UI.Config.DocumentTemplate
{
    public interface ConfigDocumentTemplateUndetectedPagesModel : BaseUIModel
    {
        List<Disco.Models.Repository.DocumentTemplate> DocumentTemplates { get; set; }
    }
}
