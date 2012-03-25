#region License
// MessageSearchModel.cs is part of ReadingTool.Models
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

using System;
using MongoDB.Bson;

namespace ReadingTool.Models.Search
{
    public class MessageSearchItemModel
    {
        public string Id { get; set; }
        public dynamic From { get; set; }
        public dynamic[] To { get; set; }
        public bool IsStarred { get; set; }
        public bool IsRead { get; set; }
        public string Subject { get; set; }
        public DateTime ActualDate { get; set; }
        public string Date { get; set; }
        
        public ObjectId OwnerId { get; set; }
        public ObjectId FromId { get; set; }
        public ObjectId[] ToId { get; set; }
    }
}