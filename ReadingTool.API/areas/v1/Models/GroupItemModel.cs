﻿#region License
// GroupItemModel.cs is part of ReadingTool.API
// 
// ReadingTool.API is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.API is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.API. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion
namespace ReadingTool.API.Areas.V1.Models
{
    public class GroupItemModel
    {
        public string ItemId { get; set; }
        public string CollectionName { get; set; }
        public int? CollectionNo { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string L1Text { get; set; }
        public bool ParallelIsRtl { get; set; }
        public string L2Text { get; set; }
        public bool IsParallel { get; set; }
    }
}