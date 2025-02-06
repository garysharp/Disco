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
    using Metadata = ExportFieldMetadata<LogLiveEvent>;

    public class LogExportContext : IExportContext<LogExportOptions, LogLiveEvent>
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
        private LogExportContext()
        {
        }

        public LogExportContext(string name, string description, bool timestampSuffix, LogExportOptions options)
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            TimestampSuffix = timestampSuffix;
            Options = options;
        }

        public LogExportContext(LogExportOptions options)
            : this("Log Export", null, true, options)
        {
        }

        public ExportResult Export(DiscoDataContext database, IScheduledTaskStatus status)
            => Exporter.Export(database, this, status);

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

        public List<Metadata> BuildMetadata(DiscoDataContext database, List<LogLiveEvent> records, IScheduledTaskStatus status)
        {
            const string DateFormat = "yyyy-MM-dd";
            const string DateTimeFormat = DateFormat + " HH:mm:ss";
            Func<object, string> csvStringEncoded = (o) => o == null ? null : $"\"{((string)o).Replace("\"", "\"\"")}\"";
            Func<object, string> csvToStringEncoded = (o) => o == null ? null : o.ToString();
            Func<object, string> csvCurrencyEncoded = (o) => ((decimal?)o).HasValue ? ((decimal?)o).Value.ToString("C") : null;
            Func<object, string> csvDateEncoded = (o) => ((DateTime)o).ToString(DateFormat);
            Func<object, string> csvDateTimeEncoded = (o) => ((DateTime)o).ToString(DateTimeFormat);
            Func<object, string> csvNullableDateEncoded = (o) => ((DateTime?)o).HasValue ? csvDateEncoded(o) : null;
            Func<object, string> csvNullableDateTimeEncoded = (o) => ((DateTime?)o).HasValue ? csvDateTimeEncoded(o) : null;

            var metadata = new List<Metadata>
            {
                new Metadata(nameof(LogLiveEvent.Timestamp), nameof(LogLiveEvent.Timestamp), typeof(DateTime), e => e.Timestamp, csvDateTimeEncoded),
                new Metadata(nameof(LogLiveEvent.ModuleId), nameof(LogLiveEvent.ModuleId), typeof(int), e => e.ModuleId, csvToStringEncoded),
                new Metadata(nameof(LogLiveEvent.ModuleName), nameof(LogLiveEvent.ModuleName), typeof(string), e => e.ModuleName, csvStringEncoded),
                new Metadata(nameof(LogLiveEvent.ModuleDescription), nameof(LogLiveEvent.ModuleDescription), typeof(string), e => e.ModuleDescription, csvStringEncoded),
                new Metadata(nameof(LogLiveEvent.EventTypeId), nameof(LogLiveEvent.EventTypeId), typeof(int), e => e.EventTypeId, csvToStringEncoded),
                new Metadata(nameof(LogLiveEvent.EventTypeName), nameof(LogLiveEvent.EventTypeName), typeof(string), e => e.EventTypeName, csvStringEncoded),
                new Metadata("Severity", "Severity", typeof(string), e => e.EventTypeSeverity, csvToStringEncoded),
                new Metadata("Message", "Message", typeof(string), e => e.FormattedMessage, csvStringEncoded),
            };
            if (records.Count > 0)
            {
                var argCount = records.Max(r => r.Arguments?.Length ?? 0);
                for (var i = 0; i < argCount; i++)
                {
                    var index = i;
                    var name = $"Data{i + 1:00}";
                    metadata.Add(new Metadata(name, name, typeof(string), e => (e.Arguments?.Length ?? 0) > index ? (e.Arguments[index] ?? "null") : null, csvStringEncoded));
                }
            }

            return metadata;
        }
    }
}
