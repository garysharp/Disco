using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disco.Models.BI.Job;

namespace Disco.Models.UI.Job
{
    public interface JobIndexModel : BaseUIModel
    {
        JobTableModel OpenJobs { get; set; }
        JobTableModel LongRunningJobs { get; set; }
    }
}
