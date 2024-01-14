using Disco.Models.Areas.Config.UI.DeviceFlag;
using Disco.Models.Services.Devices.DeviceFlag;
using Disco.Models.Services.Exporting;
using System.Collections.Generic;

namespace Disco.Web.Areas.Config.Models.DeviceFlag
{
    public class ExportModel : ConfigDeviceFlagExportModel
    {
        public DeviceFlagExportOptions Options { get; set; }

        public string ExportSessionId { get; set; }
        public ExportResult ExportSessionResult { get; set; }

        public List<Disco.Models.Repository.DeviceFlag> DeviceFlags { get; set; }
    }
}
