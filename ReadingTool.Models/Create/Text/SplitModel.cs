#region License
// SplitModel.cs is part of ReadingTool.Models
// 
// ReadingTool.Models is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Models is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Models. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System.ComponentModel;
using MongoDB.Bson;

namespace ReadingTool.Models.Create.Text
{
    public class SplitModel
    {
        public ObjectId TextId { get; set; }
        public bool IsParallelText { get; set; }
        public string Title { get; set; }

        [DisplayName("Start numbering with")]
        public int? StartingNumber { get; set; }

        [DisplayName("Add these additional tags")]
        public string Tags { get; set; }
    }
}