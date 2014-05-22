using Disco.Models.Services.Devices.Exporting;
using Disco.Services.Tasks;
using System.IO;

namespace Disco.Services.Devices.Exporting
{
    public class DeviceExportTaskContext
    {
        public DeviceExportOptions Options { get; private set; }

        public ScheduledTaskStatus TaskStatus { get; set; }

        public DeviceExportResult Result { get; set; }

        public DeviceExportTaskContext(DeviceExportOptions Options)
        {
            this.Options = Options;
        }
    }
}
