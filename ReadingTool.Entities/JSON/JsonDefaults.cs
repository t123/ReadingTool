#region License
// JsonDefaults.cs is part of ReadingTool.Entities
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

using MongoDB.Bson;
using ReadingTool.Common.Enums;

namespace ReadingTool.Entities.JSON
{
    public class JsonDefaults
    {
        public ObjectId LanguageId { get; set; }
        public string Language { get; set; }
        public string CollectionName { get; set; }
        public int? StartCollectionWithNumber { get; set; }
        public bool? AutoNumberCollection { get; set; }
        public string Tags { get; set; }
        public bool ShareUrl { get; set; }
        public ItemType? ItemType { get; set; }

        public JsonDefaults()
        {
            Language = string.Empty;
            CollectionName = string.Empty;
            StartCollectionWithNumber = null;
            AutoNumberCollection = false;
            Tags = string.Empty;
            ShareUrl = false;
            ItemType = null;
        }
    }
}