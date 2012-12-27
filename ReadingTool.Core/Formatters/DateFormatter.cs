using System;

namespace ReadingTool.Core.Formatters
{
    public static class DateFormatter
    {
        public static string ToHumanAgo(this DateTime date)
        {
            return (DateTime.Now - date).ToHumanAgo();
        }

        public static string ToHumanAgo(this DateTime? date)
        {
            return date.HasValue ? (DateTime.Now - date.Value).ToHumanAgo() : "never";
        }

        public static string ToHumanAgo(this TimeSpan timespan)
        {
            if(timespan.Days > 365)
            {
                var years = Math.Floor(timespan.TotalDays / 365);
                return years == 1 ? "1 year" : years + " years";
            }

            if(timespan.Days > 30)
            {
                var months = Math.Floor(timespan.TotalDays / 30);
                return months == 1 ? "1 month" : months + " months";
            }

            if(timespan.Days == 1)
            {
                return "1 day";
            }

            if(timespan.Days > 0)
            {
                return Math.Floor(timespan.TotalDays) + " days";
            }

            if(timespan.Hours == 1)
            {
                return "1 hour";
            }

            if(timespan.Hours > 0)
            {
                return Math.Floor(timespan.TotalHours) + " hours";
            }

            if(timespan.Minutes > 5)
            {
                return Math.Floor(timespan.TotalMinutes) + " minutes";
            }

            if(timespan.Minutes > 1)
            {
                return "minutes";
            }

            return "seconds";
        }
    }
}