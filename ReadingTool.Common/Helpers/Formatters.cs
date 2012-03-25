#region License
// Formatters.cs is part of ReadingTool.Common
// 
// ReadingTool.Common is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Common is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Common. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System;

namespace ReadingTool.Common.Helpers
{
    public class Formatters
    {
        public static string FormatTimespan(DateTime? date)
        {
            return date.HasValue ? FormatTimespan(DateTime.Now - date.Value) : "never";
        }

        public static string FormatTimespan(TimeSpan timespan)
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