using System;
using System.Collections.Generic;
using System.Globalization;

namespace Hood.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToDisplay(this DateTime value, bool includeTime = true)
        {
            if (includeTime)
                return value.ToShortDateString() + " at " + value.ToString("hh:mmtt", CultureInfo.InvariantCulture).ToLowerInvariant();
            else return value.ToShortDateString();
        }
        public static string ToLongDisplay(this DateTime value, bool includeTime = true)
        {
            if (includeTime)
                return value.ToLongDateString() + " at " + value.ToString("hh:mmtt", CultureInfo.InvariantCulture).ToLowerInvariant();
            else return value.ToLongDateString();
        }
        public static string ToTimeDisplay(this DateTime value)
        {
            return value.ToString("hh:mmtt", CultureInfo.InvariantCulture).ToLowerInvariant();
        }
        public static IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }
        public static string ToHumaneDate(this DateTime value)
        {
            const int SECOND = 1;
            const int MINUTE = 60 * SECOND;
            const int HOUR = 60 * MINUTE;
            const int DAY = 24 * HOUR;
            const int MONTH = 30 * DAY;

            var ts = new TimeSpan(DateTime.UtcNow.Ticks - value.Ticks);
            double delta = Math.Abs(ts.TotalSeconds);

            if (delta < 1 * MINUTE)
                return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";

            if (delta < 2 * MINUTE)
                return "a minute ago";

            if (delta < 45 * MINUTE)
                return ts.Minutes + " minutes ago";

            if (delta < 90 * MINUTE)
                return "an hour ago";

            if (delta < 24 * HOUR)
                return ts.Hours + " hours ago";

            if (delta < 48 * HOUR)
                return "yesterday";

            if (delta < 30 * DAY)
                return ts.Days + " days ago";

            if (delta < 12 * MONTH)
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "one month ago" : months + " months ago";
            }
            else
            {
                int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                return years <= 1 ? "one year ago" : years + " years ago";
            }
        }
    }
}