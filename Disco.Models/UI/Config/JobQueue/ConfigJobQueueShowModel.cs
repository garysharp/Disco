using Disco.Models.Services.Jobs.JobQueues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.UI.Config.JobQueue
{
    public interface ConfigJobQueueShowModel : BaseUIModel
    {
        IJobQueueToken Token { get; set; }

        int OpenJobCount { get; set; }
        int TotalJobCount { get; set; }

        List<Disco.Models.Repository.JobType> JobTypes { get; set; }

        bool CanDelete { get; set; }
    }
}
