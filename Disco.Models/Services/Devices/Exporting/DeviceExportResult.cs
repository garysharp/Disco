using System.IO;

namespace Disco.Models.Services.Devices.Exporting
{
    public class DeviceExportResult
    {
        public MemoryStream Result { get; set; }
        public int RecordCount { get; set; }
    }
}