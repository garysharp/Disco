using System;

namespace Disco.Models.Services.Exporting
{
    public class SavedExportSchedule
    {
        public int Version { get; set; } = 1;
        public byte WeekDays { get; set; }
        public byte StartHour { get; set; }
        public byte? EndHour { get; set; }

        public bool IncludesDay(DayOfWeek day)
            => (WeekDays & (1 << (int)day)) != 0;

        public string StartHourFriendly()
            => HourFriendly(StartHour);

        public string EndHourFriendly()
            => EndHour.HasValue ? HourFriendly(EndHour.Value) : string.Empty;

        private static string HourFriendly(int hour)
        {
            if (hour == 0)
                return "12:00 AM";
            else if (hour < 12)
                return $"{hour}:00 AM";
            else if (hour == 12)
                return "12:00 PM";
            else
                return $"{hour - 12}:00 PM";
        }
    }
}
