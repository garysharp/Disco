using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.UI.Config.DocumentTemplate
{
    public interface ConfigDocumentTemplateShowModel : BaseUIModel
    {
        Disco.Models.Repository.DocumentTemplate DocumentTemplate { get; set; }
        int StoredInstanceCount { get; set; }

        List<string> Types { get; set; }
        List<string> SubTypes { get; set; }

        List<Disco.Models.Repository.JobType> JobTypes { get; set; }
        List<Disco.Models.Repository.JobSubType> JobSubTypes { get; set; }

        List<string> Scopes { get; }
    }
}
