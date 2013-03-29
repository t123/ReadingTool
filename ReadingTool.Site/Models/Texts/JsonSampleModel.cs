﻿#region License
// JsonSampleModel.cs is part of ReadingTool.Site
// 
// ReadingTool.Site is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Site is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Site. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2013 Travis Watt
#endregion

using System.ComponentModel.DataAnnotations;

namespace ReadingTool.Site.Models.Texts
{
    public class JsonSampleModel
    {
        [Display(Name = "Number of items", Order = 1)]
        [Range(0, 100)]
        public int? NumberOfItems { get; set; }

        [Display(Name = "Default L1 Name", Order = 2)]
        public string L1Name { get; set; }

        [Display(Name = "Default L2 Name", Order = 3)]
        public string L2Name { get; set; }

        [Display(Name = "Default Collection Name", Order = 4)]
        public string CollectionName { get; set; }

        [Display(Name = "Auto Number Collection", Order = 5)]
        public bool AutoNumberCollection { get; set; }

        [Display(Name = "Start Collection With Number", Order = 6)]
        [Range(0, 1000)]
        public int? StartCollectionWithNumber { get; set; }
    }
}