#region License
// GroupMembershipItemModel.cs is part of ReadingTool.Models
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

namespace ReadingTool.Models.Search
{
    public class GroupMembershipItemModel
    {
        private ObjectId _userId;
        public ObjectId UserId
        {
            get { return _userId; }
            set
            {
                _userId = value;
                Id = value.ToString();
            }
        }

        public string Id { get; private set; }
        public string Type { get; set; }
        public dynamic User { get; set; }
    }
}