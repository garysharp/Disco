using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disco
{
    public static class DateTimeExtensions
    {
        public static string From(this DateTime moment, DateTime to, bool withoutSuffix)
        {
            var duration = to - moment;
            return duration.Humanize(!withoutSuffix);
        }
        public static string From(this DateTime moment, DateTime to)
        {
            return moment.From(to, false);
        }
        public static string From(this DateTime? moment, DateTime to, bool withoutSuffix, string nullValue)
        {
            if (moment.HasValue)
                return moment.Value.From(to, withoutSuffix);
            else
                return nullValue;
        }
        public static string From(this DateTime? moment, DateTime to, string nullValue)
        {
            if (moment.HasValue)
                return moment.Value.From(to);
            else
                return nullValue;
        }
        public static string From(this DateTime? moment, DateTime to)
        {
            return moment.From(to, "n/a");
        }
        public static string FromNow(this DateTime moment, bool withoutSuffix)
        {
            return DateTime.Now.From(moment, withoutSuffix);
        }
        public static string FromNow(this DateTime moment)
        {
            return moment.FromNow(false);
        }
        public static string FromNow(this DateTime? moment, bool withoutSuffix, string nullValue)
        {
            if (moment.HasValue)
                return moment.Value.FromNow(withoutSuffix);
            else
                return nullValue;
        }
        public static string FromNow(this DateTime? moment, string nullValue)
        {
            if (moment.HasValue)
                return moment.Value.FromNow();
            else
                return nullValue;
        }
        public static string FromNow(this DateTime? moment)
        {
            return moment.FromNow("n/a");
        }
        public static string Humanize(this TimeSpan duration, bool withSuffix)
        {
            string output = RelativeTime(duration.Ticks > 0 ? duration : duration.Negate(), true);

            if (withSuffix)
                if (duration.TotalMilliseconds > 0)
                    output = "in " + output;
                else
                    output = output + " ago";

            return output;
        }

        private static string RelativeTime(this TimeSpan difference, bool withoutSuffix)
        {
            string output;

            if (Math.Round(difference.TotalSeconds) < 45)
                output = "a few seconds";
            else if (Math.Round(difference.TotalMinutes) == 1)
                output = "a minute";
            else if (Math.Round(difference.TotalMinutes) < 45)
                output = (int)Math.Round(difference.TotalMinutes) + " minutes";
            else if (Math.Round(difference.TotalHours) == 1)
                output = "an hour";
            else if (Math.Round(difference.TotalHours) < 22)
                output = (int)Math.Round(difference.TotalHours) + " hours";
            else if (Math.Round(difference.TotalDays) == 1)
                output = "a day";
            else if (Math.Round(difference.TotalDays) <= 25)
                output = (int)Math.Round(difference.TotalDays) + " days";
            else if (Math.Round(difference.TotalDays) <= 45)
                output = "a month";
            else if (Math.Round(difference.TotalDays) < 345)
                output = (int)Math.Round(difference.TotalDays / 30) + " months";
            else if (Math.Round(difference.TotalDays / 365) == 1)
                output = "a year";
            else
                output = (int)Math.Round(difference.TotalDays / 365) + " years";

            if (!withoutSuffix)
                if (difference.TotalMilliseconds > 0)
                    output = "in " + output;
                else
                    output = output + " ago";

            return output;
        }

        public static string ToFullDateTime(this DateTime d)
        {
            return d.ToString("ddd, d MMM yyyy h:mm:sstt");
        }
        public static string ToFullDateTime(this DateTime? d, string NullValue = "N/A")
        {
            if (d.HasValue)
                return ToFullDateTime(d.Value);
            else
                return NullValue;
        }

        private const long unixEpocOffset = 621355968000000000;
        public static long ToUnixEpoc(this DateTime d)
        {
            var epoc = new DateTime(unixEpocOffset, DateTimeKind.Utc);
            var offset = d.ToUniversalTime() - epoc;
            return offset.Ticks / 10000;
        }
        public static long? ToUnixEpoc(this DateTime? d)
        {
            if (d.HasValue)
                return d.Value.ToUnixEpoc();
            else
                return null;
        }

        public static string ToISO8601(this DateTime d)
        {
            return d.ToString("yyyy-MM-ddTHH\\:mm\\:sszzz");
        }
        public static string ToISO8601(this DateTime? d)
        {
            if (d.HasValue)
                return d.Value.ToISO8601();
            else
                return null;
        }

    }
}
