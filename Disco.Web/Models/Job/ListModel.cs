using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Disco.Models.UI.Job;

namespace Disco.Web.Models.Job
{
    public class ListModel : JobListModel
    {
        public string Title { get; set; }
        public Disco.Models.BI.Job.JobTableModel JobTable { get; set; }

        public string PageTitle
        {
            get
            {
                return string.Format("{0} ({1})", Title, JobTable.Items.Count);
            }
        }
    }
}