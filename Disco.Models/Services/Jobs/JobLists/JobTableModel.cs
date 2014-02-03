using System.Collections.Generic;

namespace Disco.Models.Services.Jobs.JobLists
{
    public class JobTableModel
    {
        public bool ShowId { get; set; }
        public bool? ShowDeviceAddress { get; set; }
        public bool ShowDates { get; set; }
        public bool ShowType { get; set; }
        public bool ShowDevice { get; set; }
        public bool ShowUser { get; set; }
        public bool ShowTechnician { get; set; }
        public bool ShowLocation { get; set; }
        public bool ShowStatus { get; set; }
        
        public bool IsSmallTable { get; set; }
        public bool HideClosedJobs { get; set; }
        public bool EnablePaging { get; set; }
        public bool EnableFilter { get; set; }

        public virtual IEnumerable<JobTableItemModel> Items { get; set; }

        public JobTableModel()
        {
            ShowId = true;
            ShowDates = true;
            ShowType = true;
            ShowDevice = true;
            ShowUser = true;
            ShowTechnician = true;
            EnablePaging = true;
            EnableFilter = true;
        }
    }
}
