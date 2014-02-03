using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Disco.Models.UI.Job;
using Disco.Models.Services.Jobs.JobLists;

namespace Disco.Web.Models.Job
{
    public class IndexModel : JobIndexModel
    {
        public JobTableModel MyJobs { get; set; }
        public JobTableModel LongRunningJobs { get; set; }

        public List<Disco.Models.BI.Job.Statistics.DailyOpenedClosedItem> DailyOpenedClosedStatistics { get; set; }
    }
}