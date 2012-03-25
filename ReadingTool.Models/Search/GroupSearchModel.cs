#region License
// GroupSearchModel.cs is part of ReadingTool.Models
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

namespace ReadingTool.Models.Search
{
    public class GroupSearchItemModel
    {
        private ObjectId _groupId;
        public ObjectId GroupId
        {
            get { return _groupId; }
            set
            {
                _groupId = value;
                Id = value.ToString();
            }
        }

        public string Id { get; private set; }
        public string Name { get; set; }

        private GroupType _groupType;
        public GroupType GroupType { get { return _groupType; } set { _groupType = value; if(value == GroupType.Public) CanInfo = true; } }

        private string _associtation;
        public string Association
        {
            get { return _associtation; }
            set
            {
                _associtation = value;

                GroupMembershipType type;
                if(!Enum.TryParse(value, true, out type))
                    return;

                switch(type)
                {
                    case GroupMembershipType.Owner:
                    case GroupMembershipType.Moderator:
                        CanEdit = true;
                        CanInfo = true;
                        CanManage = true;
                        CanView = true;
                        break;

                    case GroupMembershipType.Member:
                        CanView = true;
                        CanInfo = true;
                        break;

                    case GroupMembershipType.Invitation:
                        CanInfo = true;
                        break;

                    default: break;
                }
            }
        }

        public bool CanEdit { get; private set; }
        public bool CanManage { get; private set; }
        public bool CanInfo { get; private set; }
        public bool CanView { get; private set; }
        public string Pending { get; set; }

        public GroupSearchItemModel()
        {
            Pending = "";
        }
    }
}