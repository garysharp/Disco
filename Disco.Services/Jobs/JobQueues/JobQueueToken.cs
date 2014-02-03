using Disco.Models.Repository;
using Disco.Models.Services.Jobs.JobQueues;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Disco.Services.Jobs.JobQueues
{
    public class JobQueueToken : IJobQueueToken
    {
        public JobQueue JobQueue { get; private set; }
        internal HashSet<string> SubjectIdHashes { get; private set; }
        public ReadOnlyCollection<string> SubjectIds { get; private set; }

        public static JobQueueToken FromJobQueue(JobQueue JobQueue)
        {
            string[] sg = (JobQueue.SubjectIds == null ? new string[0] : JobQueue.SubjectIds.Split(',').ToArray());

            return new JobQueueToken()
            {
                JobQueue = JobQueue,
                SubjectIdHashes = new HashSet<string>(sg.Select(i => i.ToLower())),
                SubjectIds = sg.ToList().AsReadOnly()
            };
        }
    }
}
