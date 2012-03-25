#region License
// TaskSchedule.cs is part of ReadingTool.Common
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

using System.ComponentModel;

namespace ReadingTool.Common.Enums
{
    public enum TaskSchedule
    {
        [Description("Only run once, set hour and minute (defaults to 23:59)")]
        Once = 0,

        [Description("Fixed time, set hour and minute (defaults to 23:59)")]
        FixedTime = 1,

        [Description("Periodically, every minuteS minutes (defaults to 60")]
        Periodically = 2,

        [Description("Hourly, runs every hour at minute (defaults to on the hour)")]
        Hourly = 3
    }
}
