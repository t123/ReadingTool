#region License
// GroupModel.cs is part of ReadingTool.Models
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

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using MongoDB.Bson;
using ReadingTool.Common.Attributes;
using ReadingTool.Common.Enums;

namespace ReadingTool.Models.Create.Group
{
    public class GroupModel
    {
        public ObjectId GroupId { get; set; }

        [StringLength(30)]
        [Required]
        [Remote("GroupNameInUse", "RemoteValidator", HttpMethod = "POST", AdditionalFields = "GroupId")]
        public string Name { get; set; }

        [StringLength(1000)]
        [DisplayName("About the group")]
        public string About { get; set; }

        public string Tags { get; set; }

        [Help("Groups can either be public or by invitation. In public groups a moderator must approve a request, for private groups the user must " +
            "accept membership.")]
        [Required]
        [DisplayName("Type of group")]
        public GroupType Type { get; set; }

        [Help("Users you'd like to send an invitation to.")]
        [DisplayName("Invite these users")]
        public string Invitations { get; set; }
    }
}