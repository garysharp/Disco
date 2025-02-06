using Disco.Models.Areas.Config.UI.UserFlag;
using Disco.Models.Services.Exporting;
using Disco.Models.Services.Users.UserFlags;
using System;
using System.Collections.Generic;

namespace Disco.Web.Areas.Config.Models.UserFlag
{
    public class ExportModel : ConfigUserFlagExportModel
    {
        public UserFlagExportOptions Options { get; set; }

        public Guid? ExportId { get; set; }
        public ExportResult ExportResult { get; set; }

        public List<Disco.Models.Repository.UserFlag> UserFlags { get; set; }
    }
}
