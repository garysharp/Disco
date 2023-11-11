using Disco.Models.Services.Devices.Exporting;
using Disco.Models.Services.Exporting;
using System.Collections.Generic;

namespace Disco.Models.UI.Device
{
    public interface DeviceExportModel : BaseUIModel
    {
        DeviceExportOptions Options { get; set; }

        string ExportSessionId { get; set; }
        ExportResult ExportSessionResult { get; set; }

        IEnumerable<KeyValuePair<int, string>> DeviceBatches { get; set; }
        IEnumerable<KeyValuePair<int, string>> DeviceModels { get; set; }
        IEnumerable<KeyValuePair<int, string>> DeviceProfiles { get; set; }
    }
}