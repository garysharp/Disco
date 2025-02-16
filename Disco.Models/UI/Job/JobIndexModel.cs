using Disco.Models.ClientServices;
using Disco.Models.Services.Jobs.JobLists;
using System.Collections.Generic;

namespace Disco.Models.UI.Job
{
    public interface JobIndexModel : BaseUIModel
    {
        JobTableModel MyJobs { get; set; }
        JobTableModel StaleJobs { get; set; }
        List<EnrolResponse> PendingEnrolments { get; set; }
    }
}
