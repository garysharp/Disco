using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Disco.Web.Areas.API.Models.DeviceBatch
{
    public class DeviceBatchTimelineEventSource
    {
        public string wikiURL { get; set; }
        public string wikiSection { get; set; }
        public string dateTimeFormat { get { return "iso8601"; } }
        public DeviceBatchTimelineEvent[] events { get; set; }
    }
}