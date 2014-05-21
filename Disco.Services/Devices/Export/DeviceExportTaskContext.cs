using Disco.Models.Services.Devices.Exporting;
using Disco.Services.Tasks;
using System.IO;

namespace Disco.Services.Devices.Export
{
    public class DeviceExportTaskContext
    {
        public DeviceExportOptions Options { get; private set; }

        public ScheduledTaskStatus TaskStatus { get; set; }

        public MemoryStream CsvResult { get; set; }

        public DeviceExportTaskContext(DeviceExportOptions Options)
        {
            this.Options = Options;
        }
    }
}
