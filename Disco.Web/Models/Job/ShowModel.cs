using Disco.Models.BI.Job;
using Disco.Models.Services.Jobs.JobLists;
using Disco.Models.UI.Job;
using Disco.Web.Extensions;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Disco.Web.Models.Job
{
    public class ShowModel : JobShowModel
    {
        public Disco.Models.Repository.Job Job { get; set; }

        public TimeSpan? LongRunning { get; set; }

        public List<Disco.Models.Repository.DocumentTemplate> AvailableDocumentTemplates { get; set; }
        public List<Disco.Models.Repository.JobSubType> UpdatableJobSubTypes { get; set; }
        public List<Disco.Models.Repository.JobQueue> AvailableQueues { get; set; }

        public List<SelectListItem> DocumentTemplatesSelectListItems
        {
            get
            {
                var list = new List<SelectListItem>();
                list.Add(new SelectListItem() { Selected = true, Value = string.Empty, Text = "Generate Document" });
                list.AddRange(this.AvailableDocumentTemplates.ToSelectListItems());
                return list;
            }
        }


        public LocationModes LocationMode { get; set; }
        public List<JobLocationReference> LocationOptions { get; set; }
    }
}