using Disco.Models.Services.Devices;
using Disco.Models.Services.Exporting;
using System;
using System.Collections.Generic;

namespace Disco.Models.UI.Device
{
    public interface DeviceExportModel : BaseUIModel
    {
        DeviceExportOptions Options { get; set; }

        Guid? ExportId { get; set; }
        ExportResult ExportResult { get; set; }

        IEnumerable<KeyValuePair<int, string>> DeviceBatches { get; set; }
        IEnumerable<KeyValuePair<int, string>> DeviceModels { get; set; }
        IEnumerable<KeyValuePair<int, string>> DeviceProfiles { get; set; }
    }
}