using Disco.Models.Services.Devices;
using Disco.Models.Services.Exporting;
using Disco.Models.Services.Users.UserFlags;
using Disco.Models.UI;
using Disco.Models.UI.Shared;
using System;
using System.Collections.Generic;

namespace Disco.Models.Areas.Config.UI.UserFlag
{
    public interface ConfigUserFlagExportModel : BaseUIModel
    {
        UserFlagExportOptions Options { get; set; }

        Guid? ExportId { get; set; }
        ExportResult ExportResult { get; set; }

        List<Repository.UserFlag> UserFlags { get; set; }

        SharedExportFieldsModel<UserFlagExportOptions> Fields { get; set; }
    }
}
