#region License
// LoginHistory.cs is part of ReadingTool.Entities
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

namespace ReadingTool.Entities
{
    public enum LoginHistoryType
    {
        Success = 0,
        Failed = 1,
        UnknownUser = 2,
    }

    public class LoginHistory
    {
        [BsonId]
        public ObjectId LoginHistoryId { get; set; }
        public ObjectId UserId { get; set; }
        public DateTime At { get; set; }
        public LoginHistoryType Type { get; set; }

        public LoginHistory()
        {
            At = DateTime.Now;
        }
    }
}