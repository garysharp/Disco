using Disco.Models.Services.Jobs.JobLists;

namespace Disco.Models.UI.Job
{
    public interface JobIndexModel : BaseUIModel
    {
        JobTableModel MyJobs { get; set; }
        JobTableModel LongRunningJobs { get; set; }
    }
}
