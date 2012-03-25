#region License
// Group.cs is part of ReadingTool.Entities
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
    public class Group
    {
        public const string CollectionName = @"Groups";

        [BsonId]
        public ObjectId GroupId { get; set; }
        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                NameLower = value.ToLowerInvariant();
            }
        }

        public string NameLower { get; private set; }

        public string About { get; set; }
        private string[] _tags;
        public string[] Tags { get { return _tags ?? new string[0]; } set { _tags = value; } }
        public GroupType Type { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public ObjectId Owner { get; set; }

        public bool ShouldSerializeTags()
        {
            return Tags != null && Tags.Length > 0;
        }
    }
}