using Disco.Models.Areas.Config.UI.UserFlag;
using Disco.Models.Services.Exporting;
using Disco.Models.Services.Users.UserFlags;
using System.Collections.Generic;

namespace Disco.Web.Areas.Config.Models.UserFlag
{
    public class ExportModel : ConfigUserFlagExportModel
    {
        public UserFlagExportOptions Options { get; set; }

        public string ExportSessionId { get; set; }
        public ExportResult ExportSessionResult { get; set; }

        public List<Disco.Models.Repository.UserFlag> UserFlags { get; set; }
    }
}
