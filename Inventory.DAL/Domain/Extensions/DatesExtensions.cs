using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AECMIS.DAL.Domain.Extensions
{
    public static class DatesExtensions
    {
        public static IEnumerable<DateTime> AllDatesBetween(this DateTime start, DateTime end)
        {
            for (var day = start.Date; day <= end; day = day.AddDays(1))
                yield return day;
        }
        public static string HoursAndMins(this TimeSpan dateTime)
        {
            return dateTime.ToString(@"hh\:mm");
        }

        public static DateTime Localize(this DateTime dateTime)
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            return TimeZoneInfo.ConvertTime(dateTime, timeZoneInfo);
        }


    }
}
