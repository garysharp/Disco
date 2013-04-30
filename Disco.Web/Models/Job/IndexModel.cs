using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Disco.Models.UI.Job;

namespace Disco.Web.Models.Job
{
    public class IndexModel : JobIndexModel
    {
        public Disco.Models.BI.Job.JobTableModel OpenJobs { get; set; }
        public Disco.Models.BI.Job.JobTableModel LongRunningJobs { get; set; }
        //public Disco.Models.BI.Job.JobTableModel WaitingForUserActionJobs { get; set; }
        //public Disco.Models.BI.Job.JobTableModel ReadyForReturnJobs { get; set; }
        //public Disco.Models.BI.Job.JobTableModel RecentlyClosedJobs { get; set; }

        public List<Disco.Models.BI.Job.Statistics.DailyOpenedClosedItem> DailyOpenedClosedStatistics { get; set; }
    }
}