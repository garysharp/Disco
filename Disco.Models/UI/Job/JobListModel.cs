using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.UI.Job
{
    public interface JobListModel : BaseUIModel
    {
        string Title { get; set; }
        Disco.Models.BI.Job.JobTableModel JobTable { get; set; }
    }
}
