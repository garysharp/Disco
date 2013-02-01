using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disco.Web.Models.Job
{
    public class IndexModel
    {
        public Disco.Models.BI.Job.JobTableModel OpenJobs { get; set; }
        public Disco.Models.BI.Job.JobTableModel LongRunningJobs { get; set; }
        //public Disco.Models.BI.Job.JobTableModel WaitingForUserActionJobs { get; set; }
        //public Disco.Models.BI.Job.JobTableModel ReadyForReturnJobs { get; set; }
        //public Disco.Models.BI.Job.JobTableModel RecentlyClosedJobs { get; set; }
    }
}