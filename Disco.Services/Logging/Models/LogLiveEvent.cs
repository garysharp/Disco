﻿using System;
using Disco.Models.Exporting;
using Newtonsoft.Json;

namespace Disco.Services.Logging.Models
{
    public class LogLiveEvent : IExportRecord
    {
        public int ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string ModuleDescription { get; set; }
        public int EventTypeId { get; set; }
        public string EventTypeName { get; set; }
        public int EventTypeSeverity { get; set; }

        public DateTime Timestamp { get; set; }
        public object[] Arguments { get; set; }
        public string FormattedMessage { get; set; }
        public string FormattedTimestamp { get; set; }
        public bool UseDisplay { get; set; }

        public static LogLiveEvent Create(LogBase logModule, LogEventType eventType, DateTime Timestamp, string jsonArguments)
        {
            object[] Arguments = null;
            if (jsonArguments != null)
            {
                Arguments = JsonConvert.DeserializeObject<object[]>(jsonArguments);
            }
            return Create(logModule, eventType, Timestamp, Arguments);
        }

        public static LogLiveEvent Create(LogBase logModule, LogEventType eventType, DateTime Timestamp, params object[] Arguments)
        {
            return new LogLiveEvent()
            {
                ModuleId = logModule.ModuleId,
                ModuleName = logModule.ModuleName,
                ModuleDescription = logModule.ModuleDescription,
                EventTypeId = eventType.Id,
                EventTypeName = eventType.Name,
                EventTypeSeverity = eventType.Severity,
                Timestamp = Timestamp,
                Arguments = Arguments,
                FormattedMessage = eventType.FormatMessage(Arguments),
                FormattedTimestamp = Timestamp.ToString("dd/MM/yyy hh:mm:ss tt"),
                UseDisplay = eventType.UseDisplay
            };
        }

    }
}
