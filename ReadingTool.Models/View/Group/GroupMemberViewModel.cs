#region License
// GroupMemberViewModel.cs is part of ReadingTool.Models
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
using ReadingTool.Common.Enums;

namespace ReadingTool.Models.View.Group
{
    public class GroupMemberViewModel
    {
        public ObjectId GroupMemberId { get; set; }
        public ObjectId UserId { get; set; }
        public ObjectId GroupId { get; set; }
        public GroupMembershipType Type { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }
}