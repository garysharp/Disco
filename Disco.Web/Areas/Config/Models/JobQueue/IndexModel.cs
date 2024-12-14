using Disco.Models.Services.Jobs.JobQueues;
using Disco.Models.UI.Config.JobQueue;
using System.Collections.Generic;

namespace Disco.Web.Areas.Config.Models.JobQueue
{
    public class IndexModel : ConfigJobQueueIndexModel
    {
        public List<IJobQueueToken> Tokens { get; set; }
    }
}