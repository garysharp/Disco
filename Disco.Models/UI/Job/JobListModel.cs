using Disco.Models.Services.Jobs.JobLists;

namespace Disco.Models.UI.Job
{
    public interface JobListModel : BaseUIModel
    {
        string Title { get; set; }
        JobTableModel JobTable { get; set; }
    }
}