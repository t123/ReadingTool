#region License
// SystemTaskModel.cs is part of ReadingTool
// 
// ReadingTool is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using ReadingTool.Common.Enums;

namespace ReadingTool.Areas.Admin.Models
{
    public class SystemTaskModel
    {
        public ObjectId SystemTaskId { get; set; }

        [Required]
        public string Name { get; set; }

        public bool IsActive { get; set; }
        public DateTime? LastRunDate { get; set; }
        public DateTime? NextRunDate { get; set; }
        public bool IsRunning { get; set; }

        [Required]
        public string ClassName { get; set; }

        [Required]
        public TaskSchedule Schedule { get; set; }

        [Display(Name = "How often in minutes?")]
        public int? Minutes { get; set; }

        [Display(Name = "At which hour?")]
        public int? Hour { get; set; }

        [Display(Name = "At which minute?")]
        public int? Minute { get; set; }

        [Display(Name = "Max failures for notification")]
        public int ConsecutiveFailures { get; set; }
        public int MaximumFailures { get; set; }
    }
}
