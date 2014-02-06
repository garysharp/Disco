using Disco.Models.Services.Jobs.JobLists;
using Disco.Models.Services.Searching;
using System.Collections.Generic;

namespace Disco.Models.UI.Search
{
    public interface SearchQueryModel : BaseUIModel
    {
        string FriendlyTerm { get; set; }
        string Term { get; set; }
        bool Success { get; set; }
        string ErrorMessage { get; set; }
        List<DeviceSearchResultItem> Devices { get; set; }
        JobTableModel Jobs { get; set; }
        List<UserSearchResultItem> Users { get; set; }
    }
}
