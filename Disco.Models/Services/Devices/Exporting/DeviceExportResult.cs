using System.IO;

namespace Disco.Models.Services.Devices.Exporting
{
    public class DeviceExportResult
    {
        public MemoryStream CsvResult { get; set; }
        public int RecordCount { get; set; }
    }
}