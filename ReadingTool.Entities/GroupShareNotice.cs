﻿#region License
// GroupShareNotice.cs is part of ReadingTool.Entities
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
    public class GroupShareNotice
    {
        public const string DbCollectionName = @"GroupShareNotices";

        [BsonId]
        public ObjectId GroupShareNoticeId { get; set; }
        public DateTime Created { get; set; }
        public ObjectId GroupId { get; set; }
        public ObjectId UserId { get; set; }
        public ObjectId ItemId { get; set; }
        public string Title { get; set; }
        public string CollectionName { get; set; }
        public string Username { get; set; }
        public string LanguageName { get; set; }
        public ShareDirection ShareDirection { get; set; }
        public ItemType ItemType { get; set; }
    }
}
