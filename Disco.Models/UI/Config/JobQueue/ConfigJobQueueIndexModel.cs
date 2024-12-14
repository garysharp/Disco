using Disco.Models.Services.Jobs.JobQueues;
using System.Collections.Generic;

namespace Disco.Models.UI.Config.JobQueue
{
    public interface ConfigJobQueueIndexModel : BaseUIModel
    {
        List<IJobQueueToken> Tokens { get; set; }
    }
}
