using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disco.Web.Models.Search
{
    public class QueryModel
    {
        public string FriendlyTerm { get; set; }
        public string Term { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public List<Disco.Models.BI.Search.DeviceSearchResultItem> Devices { get; set; }
        public Disco.Models.BI.Job.JobTableModel Jobs { get; set; }
        public List<Disco.Models.BI.Search.UserSearchResultItem> Users { get; set; }
    }
}