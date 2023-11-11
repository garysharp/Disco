using Disco.Models.Services.Exporting;
using Disco.Models.Services.Users.UserFlags;
using Disco.Models.UI;
using System.Collections.Generic;

namespace Disco.Models.Areas.Config.UI.UserFlag
{
    public interface ConfigUserFlagExportModel : BaseUIModel
    {
        UserFlagExportOptions Options { get; set; }

        string ExportSessionId { get; set; }
        ExportResult ExportSessionResult { get; set; }

        List<Repository.UserFlag> UserFlags { get; set; }
    }
}
