using Disco.Data.Repository;
using Disco.Models.Exporting;
using Disco.Models.Services.Exporting;
using Disco.Models.Services.Logging;
using Disco.Services.Exporting;
using Disco.Services.Logging.Models;
using Disco.Services.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Disco.Services.Logging
{
    public class LogExport : IExport<LogExportOptions, LogLiveEvent>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool TimestampSuffix { get; set; }
        public LogExportOptions Options { get; set; }

        public string SuggestedFilenamePrefix { get; } = "DiscoIctLogs";
        public string ExcelWorksheetName { get; } = "Disco ICT Logs";
        public string ExcelTableName { get; } = "DiscoIctLogs";

        [JsonConstructor]
        private LogExport()
        {
        }

        public LogExport(string name, string description, bool timestampSuffix, LogExportOptions options)
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            TimestampSuffix = timestampSuffix;
            Options = options;
        }

        public LogExport(LogExportOptions options)
            : this("Log Export", null, true, options)
        {
        }

        public ExportResult Export(DiscoDataContext database, IScheduledTaskStatus status)
            => Exporter.Export(this, database, status);

        public List<LogLiveEvent> BuildRecords(DiscoDataContext database, IScheduledTaskStatus status)
        {
            var logRetriever = new ReadLogContext()
            {
                Start = Options.StartDate,
                End = Options.EndDate,
                Module = Options.ModuleId,
                EventTypes = Options.EventTypeIds,
                Take = Options.Take,
            };

            return logRetriever.Query(database);
        }

        public ExportMetadata<LogLiveEvent> BuildMetadata(DiscoDataContext database, List<LogLiveEvent> records, IScheduledTaskStatus status)
        {
            var metadata = new ExportMetadata<LogLiveEvent>
            {
                { nameof(LogLiveEvent.Timestamp), r => r.Timestamp },
                { nameof(LogLiveEvent.ModuleId), r => r.ModuleId },
                { nameof(LogLiveEvent.ModuleName), r => r.ModuleName },
                { nameof(LogLiveEvent.ModuleDescription), r => r.ModuleDescription },
                { nameof(LogLiveEvent.EventTypeId), r => r.EventTypeId },
                { nameof(LogLiveEvent.EventTypeName), r => r.EventTypeName },
                { "Severity", r => r.EventTypeSeverity },
                { "Message", r => r.FormattedMessage }
            };

            if (records.Count > 0)
            {
                var argCount = records.Max(r => r.Arguments?.Length ?? 0);
                for (var i = 0; i < argCount; i++)
                {
                    var index = i;
                    var name = $"Data{i + 1:00}";
                    metadata.Add(name, r => (r.Arguments?.Length ?? 0) > index ? (r.Arguments[index] ?? "null") : null);
                }
            }

            return metadata;
        }
    }
}
