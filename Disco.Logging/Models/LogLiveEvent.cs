using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Disco.Logging.Models
{
    public class LogLiveEvent
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

        public static LogLiveEvent Create(LogBase logModule, Models.LogEventType eventType, DateTime Timestamp, string jsonArguments)
        {
            object[] Arguments = null;
            if (jsonArguments != null)
            {
                var alArguments = fastJSON.JSON.Instance.Parse(jsonArguments) as ArrayList;
                if (alArguments != null)
                {
                    Arguments = alArguments.ToArray();
                }
            }
            return Create(logModule, eventType, Timestamp, Arguments);
        }

        public static LogLiveEvent Create(LogBase logModule, Models.LogEventType eventType, DateTime Timestamp, params object[] Arguments)
        {
            return new Models.LogLiveEvent()
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
