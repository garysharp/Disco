using Disco.Models.Services.Devices;
using Disco.Models.Services.Exporting;
using Disco.Models.UI.Device;
using Disco.Models.UI.Shared;
using System;
using System.Collections.Generic;

namespace Disco.Web.Models.Device
{
    public class ExportModel : DeviceExportModel
    {
        public DeviceExportOptions Options { get; set; }

        public Guid? ExportId { get; set; }
        public ExportResult ExportResult { get; set; }

        public IEnumerable<KeyValuePair<int, string>> DeviceBatches { get; set; }
        public IEnumerable<KeyValuePair<int, string>> DeviceModels { get; set; }
        public IEnumerable<KeyValuePair<int, string>> DeviceProfiles { get; set; }

        public SharedExportFieldsModel<DeviceExportOptions> Fields { get; set; }
    }
}