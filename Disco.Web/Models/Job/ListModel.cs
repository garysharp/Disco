using Disco.Models.Services.Jobs.JobLists;
using Disco.Models.UI.Job;
using System.Linq;

namespace Disco.Web.Models.Job
{
    public class ListModel : JobListModel
    {
        public string Title { get; set; }
        public JobTableModel JobTable { get; set; }

        public string PageTitle
        {
            get
            {
                return string.Format("{0} ({1})", Title, JobTable.Items.Count());
            }
        }
    }
}