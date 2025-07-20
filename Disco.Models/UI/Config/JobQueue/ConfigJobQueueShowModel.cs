using Disco.Models.Services.Jobs.JobQueues;
using System.Collections.Generic;

namespace Disco.Models.UI.Config.JobQueue
{
    public interface ConfigJobQueueShowModel : BaseUIModel
    {
        IJobQueueToken Token { get; set; }

        int OpenJobCount { get; set; }
        int TotalJobCount { get; set; }

        IEnumerable<KeyValuePair<string, string>> Icons { get; set; }
        IEnumerable<KeyValuePair<string, string>> ThemeColours { get; set; }

        List<Repository.JobType> JobTypes { get; set; }

        bool CanDelete { get; set; }
    }
}
