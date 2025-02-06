using Disco.Models.Services.Devices.DeviceFlag;
using Disco.Models.Services.Exporting;
using Disco.Models.UI;
using System;
using System.Collections.Generic;

namespace Disco.Models.Areas.Config.UI.DeviceFlag
{
    public interface ConfigDeviceFlagExportModel : BaseUIModel
    {
        DeviceFlagExportOptions Options { get; set; }

        Guid? ExportId { get; set; }
        ExportResult ExportResult { get; set; }

        List<Repository.DeviceFlag> DeviceFlags { get; set; }
    }
}
