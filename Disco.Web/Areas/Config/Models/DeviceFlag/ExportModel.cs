using Disco.Models.Areas.Config.UI.DeviceFlag;
using Disco.Models.Services.Devices.DeviceFlag;
using Disco.Models.Services.Exporting;
using Disco.Models.UI.Shared;
using System;
using System.Collections.Generic;

namespace Disco.Web.Areas.Config.Models.DeviceFlag
{
    public class ExportModel : ConfigDeviceFlagExportModel
    {
        public DeviceFlagExportOptions Options { get; set; }

        public Guid? ExportId { get; set; }
        public ExportResult ExportResult { get; set; }

        public List<Disco.Models.Repository.DeviceFlag> DeviceFlags { get; set; }

        public SharedExportFieldsModel<DeviceFlagExportOptions> Fields { get; set; }
    }
}
