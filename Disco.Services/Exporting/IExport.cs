using Disco.Data.Repository;
using Disco.Models.Exporting;
using Disco.Models.Services.Exporting;
using Disco.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Disco.Services.Exporting
{
    public interface IExport
    {
        Guid Id { get; set; }
        [JsonIgnore]
        string Name { get; }

        ExportResult Export(DiscoDataContext database, IScheduledTaskStatus status);
    }

    public interface IExport<T, R>
        : IExport
        where T : IExportOptions, new()
        where R : IExportRecord
    {
        [JsonIgnore]
        string FilenamePrefix { get; }
        [JsonIgnore]
        string ExcelWorksheetName { get; }
        [JsonIgnore]
        string ExcelTableName { get; }

        T Options { get; set; }

        List<R> BuildRecords(DiscoDataContext database, IScheduledTaskStatus status);
        ExportMetadata<R> BuildMetadata(DiscoDataContext database, List<R> records, IScheduledTaskStatus status);
    }
}
