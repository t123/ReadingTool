#region License
// GroupsController.cs is part of ReadingTool.API
// 
// ReadingTool.API is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.API is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.API. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FluentMongo.Linq;
using MongoDB.Bson;
using ReadingTool.API.Areas.V1.Common;
using ReadingTool.API.Areas.V1.Models;
using ReadingTool.API.Common;
using ReadingTool.Common.Enums;
using ReadingTool.Entities;
using ReadingTool.Services;

namespace ReadingTool.API.Areas.V1.Controllers
{
    public class GroupsController : BaseController
    {
        private ObjectId[] UserGroups()
        {
            var types = new[] { GroupMembershipType.Moderator, GroupMembershipType.Member, GroupMembershipType.Owner };

            return _db.GetCollection<GroupMember>(Collections.GroupMembers)
                .AsQueryable()
                .Where(x => x.UserId == _userId && types.Contains(x.Type))
                .Select(x => x.GroupId)
                .ToArray();
        }

        public JsonNetResult List()
        {
            var groupIds = UserGroups();

            if(Request.QueryString["count"] != null)
            {
                var count = _db.GetCollection<Group>(Collections.Groups)
                .AsQueryable()
                .Count(x => groupIds.Contains(x.GroupId));

                return new JsonNetResult() { Data = new CountResponse() { Count = count } };
            }

            var data = _db.GetCollection<Group>(Collections.Groups)
                .AsQueryable()
                .Where(x => groupIds.Contains(x.GroupId))
                .OrderBy(x => x.Name)
                .Skip((_page - 1) * _maxItemsPerPage)
                .Take(_maxItemsPerPage)
                ;

            var mapped = Mapper.Map<IEnumerable<Group>, IEnumerable<GroupModel>>(data);
            var reply = new GroupResponse()
            {
                StatusCode = StatusCode.Ok,
                Groups = mapped.ToArray()
            };

            return new JsonNetResult() { Data = reply };
        }

        public JsonNetResult ListItems(string groupId)
        {
            var groupIds = UserGroups();
            var id = new ObjectId(groupId);

            if(!groupIds.Contains(id))
            {
                return new JsonNetResult() { Data = new GroupItemsResponse { StatusCode = StatusCode.BadCredentials, StatusMessage = "User may not access group" } };
            }

            if(Request.QueryString["count"] != null)
            {
                var count = _db.GetCollection<Item>(Collections.Items)
                .AsQueryable()
                .Count(x => x.GroupId.Contains(id));

                return new JsonNetResult() { Data = new CountResponse() { Count = count } };
            }

            var data = _db.GetCollection<Item>(Collections.Items)
                .AsQueryable()
                .Where(x => x.GroupId.Contains(id))
                .OrderBy(x => x.LanguageId)
                .ThenBy(x => x.CollectionName)
                .ThenBy(x => x.CollectionNo)
                .ThenBy(x => x.Title)
                .Skip((_page - 1) * _maxItemsPerPage)
                .Take(_maxItemsPerPage)
                ;

            var mapped = Mapper.Map<IEnumerable<Item>, IEnumerable<GroupItemModel>>(data);
            var reply = new GroupItemsResponse()
            {
                StatusCode = StatusCode.Ok,
                Items = mapped.ToArray()
            };

            return new JsonNetResult() { Data = reply };
        }

        public JsonNetResult ListItemsModified(string groupId, string modified)
        {
            DateTime dt = DateTime.ParseExact(modified, "yyyyMMddHHmmss", null);
            var groupIds = UserGroups();
            var id = new ObjectId(groupId);

            if(!groupIds.Contains(id))
            {
                return new JsonNetResult() { Data = new GroupItemsResponse { StatusCode = StatusCode.BadCredentials, StatusMessage = "User may not access group" } };
            }

            var itemIds = _db.GetCollection<GroupShareNotice>(Collections.GroupShareNotices)
                .AsQueryable()
                .Where(x => x.ShareDirection == ShareDirection.Share && x.GroupId == id && x.Created >= dt)
                .Select(x => x.ItemId)
                .ToArray();

            if(Request.QueryString["count"] != null)
            {
                var count = _db.GetCollection<Item>(Collections.Items)
                    .AsQueryable()
                    .Count(x => x.GroupId.Contains(id) && itemIds.Contains(x.ItemId));

                return new JsonNetResult() { Data = new CountResponse() { Count = count } };
            }

            var data = _db.GetCollection<Item>(Collections.Items)
                .AsQueryable()
                .Where(x => x.GroupId.Contains(id) && itemIds.Contains(x.ItemId))
                .OrderBy(x => x.ItemId)
                .Skip((_page - 1) * _maxItemsPerPage)
                .Take(_maxItemsPerPage)
                ;

            var mapped = Mapper.Map<IEnumerable<Item>, IEnumerable<GroupItemModel>>(data);
            var reply = new GroupItemsResponse()
            {
                StatusCode = StatusCode.Ok,
                Items = mapped.ToArray()
            };

            return new JsonNetResult() { Data = reply };
        }
    }
}
