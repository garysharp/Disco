using Disco.Models.Repository;
using System.Collections.ObjectModel;

namespace Disco.Models.Services.Jobs.JobQueues
{
    public interface IJobQueueToken
    {
        JobQueue JobQueue { get; }
        ReadOnlyCollection<string> SubjectIds { get; }
    }
}