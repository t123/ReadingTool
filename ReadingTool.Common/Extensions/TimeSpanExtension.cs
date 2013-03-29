#region License
// TimeSpanExtension.cs is part of ReadingTool.Common
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
// Copyright (C) 2013 Travis Watt
#endregion

using System;

namespace ReadingTool.Common.Extensions
{
    public static class TimeSpanExtension
    {
        public static string ToSince(this TimeSpan? timespan, string append = "", string isNullMessage = "")
        {
            if(timespan.HasValue)
            {
                return timespan.Value.ToSince(append);
            }

            return string.IsNullOrEmpty(isNullMessage) ? "never" : isNullMessage;
        }

        public static string ToSince(this TimeSpan timespan, string append = "")
        {
            string message = "";
            if(timespan.Days > 365)
            {
                var years = Math.Floor(timespan.TotalDays / 365);
                message = years == 1 ? "1 year" : years + " years";
            }
            else if(timespan.Days > 30)
            {
                var months = Math.Floor(timespan.TotalDays / 30);
                message = months == 1 ? "1 month" : months + " months";
            }
            else if(timespan.Days == 1)
            {
                message = "1 day";
            }
            else if(timespan.Days > 0)
            {
                message = Math.Floor(timespan.TotalDays) + " days";
            }
            else if(timespan.Hours == 1)
            {
                message = "1 hour";
            }
            else if(timespan.Hours > 0)
            {
                message = Math.Floor(timespan.TotalHours) + " hours";
            }
            else if(timespan.Minutes > 5)
            {
                message = Math.Floor(timespan.TotalMinutes) + " minutes";
            }
            else if(timespan.Minutes > 1)
            {
                message = "minutes";
            }
            else
            {
                message = "seconds";
            }

            return string.IsNullOrEmpty(append) ? message : message + " " + append;
        }
    }
}
