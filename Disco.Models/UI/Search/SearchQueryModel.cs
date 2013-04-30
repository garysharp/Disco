using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco.Models.UI.Search
{
    public interface SearchQueryModel : BaseUIModel
    {
        string FriendlyTerm { get; set; }
        string Term { get; set; }
        bool Success { get; set; }
        string ErrorMessage { get; set; }
        List<Disco.Models.BI.Search.DeviceSearchResultItem> Devices { get; set; }
        Disco.Models.BI.Job.JobTableModel Jobs { get; set; }
        List<Disco.Models.BI.Search.UserSearchResultItem> Users { get; set; }
    }
}
