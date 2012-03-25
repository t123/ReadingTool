#region License
// ItemSearchModel.cs is part of ReadingTool.Models
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

using MongoDB.Bson;
using ReadingTool.Common.Enums;

namespace ReadingTool.Models.Search
{
    public class ItemSearchItemModel
    {
        public ItemType ItemType { get; set; }
        public string Language { get; set; }
        public string LanguageColour { get; set; }
        public string Title { get; set; }
        public string CollectionName { get; set; }
        public int? CollectionNo { get; set; }
        public string Id { get; set; }
        public string LastSeen { get; set; }
        public bool IsParallel { get; set; }
        public bool HasAudio { get; set; }
        public bool IsShared { get; set; }
        public string SharedGroups { get; set; }
        public ObjectId[] GroupId { get; set; }
    }
}