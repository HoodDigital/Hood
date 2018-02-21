using System;
using System.Collections.Generic;
using System.Globalization;

namespace Hood.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToDisplay(this DateTime value, bool includeTime = true)
        {
            return value.ToShortDateString() + " at " + value.ToString("hh:mmtt", CultureInfo.InvariantCulture).ToLowerInvariant();
        }
        public static string ToLongDisplay(this DateTime value, bool includeTime = true)
        {
            return value.ToLongDateString() + " at " + value.ToString("hh:mmtt", CultureInfo.InvariantCulture).ToLowerInvariant();
        }
        public static string ToTimeDisplay(this DateTime value, bool includeTime = true)
        {
            return value.ToString("hh:mmtt", CultureInfo.InvariantCulture).ToLowerInvariant();
        }
        public static IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }
    }
}