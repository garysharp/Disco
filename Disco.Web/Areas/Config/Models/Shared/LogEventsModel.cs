using System;
using System.Collections.Generic;

namespace Disco.Web.Areas.Config.Models.Shared
{
    public class LogEventsModel
    {
        public bool IsLive { get; set; }
        public Disco.Services.Logging.LogBase ModuleFilter { get; set; }
        public IEnumerable<Disco.Services.Logging.Models.LogEventType> EventTypesFilter { get; set; }
        public int? TakeFilter { get; set; }
        public DateTime? StartFilter { get; set; }
        public DateTime? EndFilter { get; set; }
        public int? ViewPortHeight { get; set; }
        public int? ViewPortWidth { get; set; }
        public string JavascriptLiveEventFunctionName { get; set; }
    }
}