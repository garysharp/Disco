using System;

namespace Disco.Models.Services.Exporting
{
    public class SavedExportSchedule
    {
        public int Version { get; set; } = 1;
        public byte WeekDays { get; set; }
        public byte StartHour { get; set; }
        public byte? EndHour { get; set; }
    }
}
