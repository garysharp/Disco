using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.UI.Job
{
    public interface JobShowModel : BaseUIModel
    {
        Repository.Job Job { get; set; }
        List<Repository.DocumentTemplate> AvailableDocumentTemplates { get; set; }
        List<Repository.JobSubType> UpdatableJobSubTypes { get; set; }
    }
}
