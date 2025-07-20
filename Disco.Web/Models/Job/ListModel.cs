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
                return $"{Title} ({JobTable.Items.Count()})";
            }
        }
    }
}