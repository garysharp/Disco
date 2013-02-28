using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Disco.Services.Logging.Models;

namespace Disco.Web.Areas.API.Models.Logs
{
    public class LogEventTypeModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Severity { get; set; }

        public static LogEventTypeModel FromLogEventType(LogEventType EventType)
        {
            return new LogEventTypeModel()
            {
                Id = EventType.Id,
                Name = EventType.Name,
                Severity = EventType.Severity
            };
        }
    }
}