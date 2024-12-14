using System;

namespace Disco.Web.Areas.API.Models.DeviceBatch
{
    public class DeviceBatchTimelineEvent
    {
        public string id { get; set; }
        public DateTime start { get; set; }
        public DateTime? end { get; set; }
        public DateTime? latestStart { get; set; }
        public DateTime? earliestEnd { get; set; }
        public bool instant { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string image { get; set; }
        public string link { get; set; }
        public string icon { get; set; }
        public string color { get; set; }
        public string textColor { get; set; }
        public string hoverText { get; set; }
        public string classname { get; set; }
        public string tapeImage { get; set; }
        public bool tapeRepeat { get; set; }
        public string caption { get; set; }
        public string eventID { get; set; }
        public string trackNum { get; set; }
    }
}