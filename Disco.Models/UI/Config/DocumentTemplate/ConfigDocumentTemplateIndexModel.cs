using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.UI.Config.DocumentTemplate
{
    public interface ConfigDocumentTemplateIndexModel : BaseUIModel
    {
        List<Disco.Models.Repository.DocumentTemplate> DocumentTemplates { get; set; }
    }
}
