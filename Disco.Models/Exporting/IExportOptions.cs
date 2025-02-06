using Disco.Models.Exporting;

namespace Disco.Models.Services.Exporting
{
    public interface IExportOptions
    {
        int Version { get; set; }
        ExportFormat Format { get; set; }
    }
}
