using Disco.Models.Repository;
using Disco.Models.Services.Exporting;
using Disco.Models.UI.Config.Export;
using System.Collections.Generic;

namespace Disco.Web.Areas.Config.Models.Export
{
    public class IndexModel : ConfigExportIndexModel
    {
        public List<SavedExport> SavedExports { get; set; }
        public Dictionary<string, string> ExportTypeNames { get; set; }
        public Dictionary<string, User> CreatedUsers { get; set; }
    }
}
