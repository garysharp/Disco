using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Disco.BI.Extensions;

namespace Disco.Web.Areas.API.Models.Job
{
    public class _DateChangeModel
    {
        public int Id { get; set; }
        public string Result { get; set; }
        public string UserDescription { get; set; }
        public string DateTimeFull { get; set; }
        public string DateTimeFriendly { get; set; }
        public string DateTimeJavascript { get; set; }
        public long DateTimeSortable { get; set; }

        public _DateChangeModel SetDateTime(DateTime? date)
        {
            this.DateTimeFriendly = date.ToFuzzy(null);
            this.DateTimeJavascript = date.ToJavascriptDateTime();
            this.DateTimeSortable = date.ToSortableDateTime();
            this.DateTimeFull = date.ToFullDateTime(null);

            return this;
        }
    }
}