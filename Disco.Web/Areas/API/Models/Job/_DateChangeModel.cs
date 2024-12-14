using System;

namespace Disco.Web.Areas.API.Models.Job
{
    public class _DateChangeModel
    {
        public int Id { get; set; }
        public string Result { get; set; }
        public string UserDescription { get; set; }
        public string DateTimeFull { get; set; }
        public string DateTimeFriendly { get; set; }
        public string DateTimeISO8601 { get; set; }
        public long DateTimeUnixEpoc { get; set; }

        public _DateChangeModel SetDateTime(DateTime? date)
        {
            this.DateTimeFriendly = date.FromNow(null);
            this.DateTimeISO8601 = date.ToISO8601();
            this.DateTimeUnixEpoc = date.ToUnixEpoc() ?? -1;
            this.DateTimeFull = date.ToFullDateTime(null);

            return this;
        }
    }
}