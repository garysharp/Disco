using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Disco.BI.Extensions;
using Disco.Web.Extensions;

namespace Disco.Web.Models.Job
{
    public class ShowModel
    {
        public Disco.Models.Repository.Job Job { get; set; }

        public List<Disco.Models.Repository.DocumentTemplate> DocumentTemplates { get; set; }
        public List<Disco.Models.Repository.JobSubType> UpdatableJobSubTypes { get; set; }

        public List<SelectListItem> DocumentTemplatesSelectListItems
        {
            get
            {
                var list = new List<SelectListItem>();
                list.Add(new SelectListItem() { Selected = true, Value = string.Empty, Text = "Generate Document" });
                list.AddRange(this.DocumentTemplates.ToSelectListItems());
                return list;
            }
        }
    }
}