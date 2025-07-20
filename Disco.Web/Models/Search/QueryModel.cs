using Disco.Models.Services.Jobs.JobLists;
using Disco.Models.Services.Searching;
using Disco.Models.UI.Search;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Disco.Web.Models.Search
{
    public class QueryModel : SearchQueryModel
    {
        public string FriendlyTerm { get; set; }
        [Required, MinLength(2)]
        public string Term { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public List<DeviceSearchResultItem> Devices { get; set; }
        public JobTableModel Jobs { get; set; }
        public List<UserSearchResultItem> Users { get; set; }
    }
}