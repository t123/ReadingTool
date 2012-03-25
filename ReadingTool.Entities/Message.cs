#region License
// Message.cs is part of ReadingTool.Entities
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
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ReadingTool.Common.Enums;

namespace ReadingTool.Entities
{
    public class Message
    {
        public const string CollectionName = @"Messages";

        [BsonId]
        public ObjectId MessageId { get; set; }
        public DateTime Created { get; set; }

        public ObjectId From { get; set; }
        public IList<ObjectId> To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsRead { get; set; } //TODO [Flags] + IsTrash -->fluentmongo doesn't like flags
        public bool IsStarred { get; set; }
        public ObjectId Owner { get; set; }
        public MessageType MessageType { get; set; }

        public Message()
        {
            To = new List<ObjectId>();
        }

        public bool ShouldSerializeTo()
        {
            return To != null && To.Count > 0;
        }

        public bool ShouldSerializeFrom()
        {
            return From != ObjectId.Empty;
        }
    }
}