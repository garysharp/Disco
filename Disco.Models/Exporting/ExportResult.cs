using System.IO;

namespace Disco.Models.Services.Exporting
{
    public class ExportResult
    {
        public MemoryStream Result { get; set; }
        public string Filename { get; set; }
        public string MimeType { get; set; }
        public int RecordCount { get; set; }
    }
}