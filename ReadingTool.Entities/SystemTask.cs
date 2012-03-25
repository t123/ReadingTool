#region License
// SystemTask.cs is part of ReadingTool.Entities
// 
// ReadingTool.Entities is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Entities is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Entities. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ReadingTool.Common.Enums;

namespace ReadingTool.Entities
{
    public class SystemTask
    {
        public const string CollectionName = @"Tasks";

        [BsonId]
        public ObjectId SystemTaskId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastRunDate { get; set; }
        public DateTime? NextRunDate { get; set; }
        public bool IsRunning { get; set; }
        public TaskSchedule Schedule { get; set; }
        public string ClassName { get; set; }
        public int ConsecutiveFailures { get; set; }
        public int MaximumFailures { get; set; }

        [BsonIgnoreIfNull]
        public int? Minutes { get; set; } //This is how many minutes between each run (for Periodically)
        [BsonIgnoreIfNull]
        public int? Hour { get; set; } //The hour to run at (Once,Fixed)
        [BsonIgnoreIfNull]
        public int? Minute { get; set; }  //The minute to run at (Once,Fixed and if set Hourly)
    }
}