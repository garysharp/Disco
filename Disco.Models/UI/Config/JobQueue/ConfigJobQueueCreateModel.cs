using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.UI.Config.JobQueue
{
    public interface ConfigJobQueueCreateModel : BaseUIModel
    {
        Repository.JobQueue JobQueue { get; set; }
    }
}
