using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Disco.Models.BI.Job.Statistics
{
    public class DailyOpenedClosedItem
    {
        public DateTime Timestamp { get; set; }
        public int TotalJobs { get; set; }
        public int OpenedJobs { get; set; }
        public int ClosedJobs { get; set; }
    }
}
