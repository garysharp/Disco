using Disco.Models.Exporting;

namespace Disco.Models.Services.Exporting
{
    public interface IExportOptions
    {
        ExportFormat Format { get; }
        string FilenamePrefix { get; }
        string ExcelWorksheetName { get; }
        string ExcelTableName { get; }
    }
}
