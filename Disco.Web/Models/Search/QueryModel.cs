using Disco.Models.BI.Search;
using Disco.Models.Services.Jobs.JobLists;
using Disco.Models.UI.Search;
using System.Collections.Generic;

namespace Disco.Web.Models.Search
{
    public class QueryModel : SearchQueryModel
    {
        public string FriendlyTerm { get; set; }
        public string Term { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public List<DeviceSearchResultItem> Devices { get; set; }
        public JobTableModel Jobs { get; set; }
        public List<UserSearchResultItem> Users { get; set; }
    }
}