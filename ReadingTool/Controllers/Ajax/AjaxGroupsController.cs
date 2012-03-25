#region License
// AjaxGroupsController.cs is part of ReadingTool
// 
// ReadingTool is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MongoDB.Bson;
using ReadingTool.Attributes;
using ReadingTool.Common;
using ReadingTool.Common.Enums;
using ReadingTool.Extensions;
using ReadingTool.Models.Search;
using ReadingTool.Models.View.User;
using ReadingTool.Services;

namespace ReadingTool.Controllers.Ajax
{
    [CustomAuthorize]
    public class AjaxGroupsController : Controller
    {
        private const string OK = @"OK";
        private const string FAIL = @"FAIL";
        private const int LIMIT = 20;

        private readonly IUserService _userService;
        private readonly IGroupService _groupService;
        private readonly ILanguageService _languageService;
        private readonly ISystemLanguageService _systemLanguageService;
        private readonly IItemService _itemService;

        public AjaxGroupsController(
            IUserService userService,
            IGroupService groupService,
            ILanguageService languageService,
            ISystemLanguageService systemLanguageService,
            IItemService itemService
            )
        {
            _userService = userService;
            _groupService = groupService;
            _languageService = languageService;
            _systemLanguageService = systemLanguageService;
            _itemService = itemService;
        }

        public void Index()
        {
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GroupInfo(string groupId)
        {
            ObjectId id;

            if(!ObjectId.TryParse(groupId, out id))
            {
                return Json(FAIL);
            }
            else
            {
                var group = _groupService.FindOne(id);

                if(group.Type == GroupType.InvitationOnly)
                {
                    //TODO fixme
                    group = _groupService.FindOneForUser(id, new[]
                    { 
                        GroupMembershipType.Moderator, 
                        GroupMembershipType.Owner, 
                        GroupMembershipType.Member, 
                        GroupMembershipType.Request, 
                        GroupMembershipType.Invitation 
                    }
                    );
                }

                var about = MarkdownHelper.Default().Transform(group.About);
                var membership = _groupService.GetMembership(group.GroupId);

                var membershipType = membership == null
                                         ? new
                                               {
                                                   type = "None",
                                                   id = ObjectId.Empty.ToString()
                                               }
                                         : new
                                               {
                                                   type = membership.Type.ToString(),
                                                   id = membership.GroupMemberId.ToString()
                                               };


                return Json(new
                                {
                                    result = OK,
                                    data = new
                                    {
                                        tags = group.Tags,
                                        about,
                                        membershipType,
                                        name = group.Name,
                                        type = group.Type.ToString()
                                    }
                                }
                    );
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonNetResult Search(
            string[] folders,
            string filter,
            int page
            )
        {
            var model = new SearchModel<GroupSearchItemModel>();
            var groups = _groupService.SearchGroups(folders, filter, true);

            var filtered = groups.Select(x =>
                                        new GroupSearchItemModel
                                        {
                                            GroupId = x.GroupId,
                                            Name = x.Name,
                                            GroupType = x.Type
                                        }
                )
                .Skip((page - 1) * LIMIT)
                .Take(LIMIT)
                .ToArray();

            var membership = _groupService
                    .FindGroupMembership(filtered.Select(x => x.GroupId))
                    .ToDictionary(x => x.GroupId, x => x);

            foreach(var group in filtered)
            {
                var association = membership.GetValueOrDefault(group.GroupId, value => null);

                if(association == null)
                {
                    group.Association = "None";
                }
                else
                {
                    group.Association = association.Type.ToString();

                    if(group.GroupType == GroupType.Public &&
                        (association.Type == GroupMembershipType.Owner || association.Type == GroupMembershipType.Moderator)
                        )
                    {
                        group.Pending = _groupService.PendingRequestForGroup(group.GroupId).ToString();
                    }
                }
            }

            model.Items = filtered;
            model.TotalItems = (int)groups.Count();
            model.TotalPages = (int)Math.Ceiling((double)model.TotalItems / (double)LIMIT);

            return new JsonNetResult() { Data = model };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonNetResult SearchMembership(
            string groupId,
            string[] folders,
            int page
            )
        {
            ObjectId id;

            if(!ObjectId.TryParse(groupId, out id))
            {
                return new JsonNetResult() { Data = FAIL };
            }

            var model = new SearchModel<GroupMembershipItemModel>();

            var members = _groupService.FindAllMembers(id, folders);

            var filtered = members
                .Skip((page - 1) * LIMIT)
                .Take(LIMIT)
                .Select(
                    x => new GroupMembershipItemModel
                             {
                                 UserId = x.UserId,
                                 Type = x.Type.ToString()
                             }
                )
                .ToArray();

            var users = FindUserForMembership(filtered);

            foreach(var membership in filtered)
            {
                var user = users.GetValueOrDefault(membership.UserId, value => UserSimpleModel.Unknown());
                membership.User = new
                                      {
                                          id = user.UserId.ToString(),
                                          username = user.Username,
                                          name = user.Name
                                      };
            }

            model.Items = filtered;
            model.TotalItems = (int)members.Count();
            model.TotalPages = (int)Math.Ceiling((double)model.TotalItems / (double)LIMIT);

            return new JsonNetResult() { Data = model };
        }

        private Dictionary<ObjectId, UserSimpleModel> FindUserForMembership(IEnumerable<GroupMembershipItemModel> members)
        {
            return _userService
                .FindAllById(members.Select(x => x.UserId))
                .ToDictionary(x => x.UserId, x => new UserSimpleModel { Name = x.Fullname, Username = x.Username, UserId = x.UserId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonNetResult GroupItems(
            string groupId,
            string[] folders,
            string filter,
            int limit,
            int page
            )
        {
            ObjectId id;

            if(!ObjectId.TryParse(groupId, out id))
            {
                return new JsonNetResult() { Data = FAIL };
            }

            var model = new SearchModel<GroupItemsSearchModel>();
            var result = _itemService.SearchItemsForGroup(id, filter, folders, limit, page);

            var filtered = result.Items.Select(x =>
                                        new GroupItemsSearchModel
                                        {
                                            ItemId = x.ItemId,
                                            CollectionName = x.CollectionName,
                                            CollectionNo = x.CollectionNo,
                                            IsText = x.ItemType == ItemType.Text,
                                            HasAudio = !string.IsNullOrEmpty(x.Url) && x.ShareUrl,
                                            IsParallel = x.IsParallel,
                                            Title = x.Title,
                                            SystemLanguageId = x.SystemLanguageId
                                        }
                )
                .ToArray();


            var systemLanguages = _systemLanguageService
                .FindAll(filtered.Select(x => x.SystemLanguageId).Distinct().ToList())
                .ToDictionary(x => x.SystemLanguageId, x => x.Name);

            foreach(var item in filtered)
            {
                item.Language = systemLanguages.GetValueOrDefault(item.SystemLanguageId, value => "Unknown");
            }

            model.Items = filtered.ToArray();
            model.TotalItems = result.Count;
            model.TotalPages = (int)Math.Ceiling((double)model.TotalItems / (double)limit);

            return new JsonNetResult() { Data = model };
        }
    }
}
