using Disco.Models.Services.Exporting;
using System.Collections.Generic;

namespace Disco.Models.UI.Config.Export
{
    public interface ConfigExportIndexModel : BaseUIModel
    {
        List<SavedExport> SavedExports { get; set; }
        Dictionary<string, string> ExportTypeNames { get; set; }
        Dictionary<string, Repository.User> CreatedUsers { get; set; }
    }
}
