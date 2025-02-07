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
        string Name { get; set; }
        string Description { get; set; }

        ExportResult Export(DiscoDataContext database, IScheduledTaskStatus status);
    }

    public interface IExport<T, R>
        : IExport
        where T : IExportOptions, new()
        where R : IExportRecord
    {
        bool TimestampSuffix { get; set; }

        [JsonIgnore]
        string SuggestedFilenamePrefix { get; }
        [JsonIgnore]
        string ExcelWorksheetName { get; }
        [JsonIgnore]
        string ExcelTableName { get; }

        T Options { get; set; }

        List<R> BuildRecords(DiscoDataContext database, IScheduledTaskStatus status);
        ExportMetadata<R> BuildMetadata(DiscoDataContext database, List<R> records, IScheduledTaskStatus status);
    }
}
