#region License
// AjaxItemsController.cs is part of ReadingTool
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
using ReadingTool.Common.Helpers;
using ReadingTool.Entities;
using ReadingTool.Entities.Identity;
using ReadingTool.Models.Search;
using ReadingTool.Services;

namespace ReadingTool.Controllers.Ajax
{
    [CustomAuthorize]
    public class AjaxItemsController : Controller
    {
        private const string OK = @"OK";
        private const string FAIL = @"FAIL";

        private readonly ISystemLanguageService _systemLanguageService;
        private readonly IItemService _itemService;
        private readonly IGroupService _groupService;
        private readonly IUserService _userService;
        private readonly ILanguageService _languageService;

        public AjaxItemsController(
            ISystemLanguageService systemLanguageService,
            IItemService itemService,
            IGroupService groupService,
            IUserService userService,
            ILanguageService languageService
            )
        {
            _systemLanguageService = systemLanguageService;
            _itemService = itemService;
            _groupService = groupService;
            _userService = userService;
            _languageService = languageService;
        }

        public void Index()
        {
        }

        //public JsonResult AutocompleteCollectionNameForLanguage(string[] types, string[] languages)
        //{
        //    var collectionNames = _itemService.AutocompleteCollectionName(types, languages);
        //    return Json(collectionNames);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult LanguagesForTexts()
        {
            var languages = _languageService.FindAllForOwner();

            return Json(
                new
                    {
                        languages = languages.Select
                    (
                        x => new
                                 {
                                     id = x.LanguageId.ToString(),
                                     name = x.Name,
                                     isRtl = x.IsRtlLanguage,
                                     code = _systemLanguageService.FindOne(x.SystemLanguageId).Code,
                                     defaultMediaUrl = x.DefaultMediaUrl
                                 }
                    )
                    }
                );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public JsonNetResult Search(
            string[] types,
            string filter,
            string[] languages,
            string[] collectionNames,
            string orderBy,
            string orderDirection,
            int limit,
            int page
            )
        {
            var model = new SearchModel<ItemSearchItemModel>();
            var result = _itemService.SearchItems(types, filter, languages, collectionNames, orderBy, orderDirection, limit, page);
            var items = result.Items.Select(x =>
                                        new ItemSearchItemModel
                                        {
                                            Language = x.LanguageName,
                                            LanguageColour = x.LanguageColour ?? "",
                                            Title = x.Title ?? "",
                                            CollectionName = x.CollectionName ?? "",
                                            CollectionNo = x.CollectionNo,
                                            Id = x.ItemId.ToString(),
                                            LastSeen = x.LastSeen.HasValue ? Formatters.FormatTimespan(x.LastSeen.Value.ToLocalTime()) + " ago" : "never",
                                            IsParallel = x.IsParallel,
                                            HasAudio = !string.IsNullOrEmpty(x.Url),
                                            IsShared = x.GroupId.Length > 0,
                                            ItemType = x.ItemType,
                                            SharedGroups =
                                                    x.GroupId.Length > 0
                                                        ? string.Join(",", (_groupService.FindAll(x.GroupId.ToArray())).Select(y => y.Name))
                                                        : ""
                                        }
                );

            model.Items = items.ToArray();
            model.TotalItems = result.Count;
            model.TotalPages = (int)Math.Ceiling((double)model.TotalItems / (double)limit);
            model.CollectionNames = _itemService.AutocompleteCollectionName(types, languages).ToArray();

            return new JsonNetResult() { Data = model };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public JsonResult AddTags(string[] items, string tagsToAdd)
        {
            try
            {
                if(items == null || items.Length == 0) return Json(OK);
                if(string.IsNullOrEmpty(tagsToAdd)) return Json(OK);

                string[] tagList = TagHelper.Split(tagsToAdd);

                foreach(var id in items)
                {
                    ObjectId tid;
                    if(!ObjectId.TryParse(id, out tid))
                        continue;

                    var item = _itemService.FindOne(tid);
                    if(item == null) continue;
                    item.Tags = TagHelper.Merge(item.Tags, tagList);
                    _itemService.Save(item);
                }

                return Json(OK);
            }
            catch
            {
                return Json(FAIL);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public JsonResult RemoveTags(string[] items, string tagsToRemove)
        {
            try
            {
                if(items == null || items.Length == 0) return Json(OK);
                if(string.IsNullOrEmpty(tagsToRemove)) return Json(OK);

                string[] tagList = TagHelper.Split(tagsToRemove);

                foreach(var id in items)
                {
                    ObjectId tid;
                    if(!ObjectId.TryParse(id, out tid))
                        continue;

                    var item = _itemService.FindOne(tid);
                    if(item == null) continue;
                    item.Tags = TagHelper.Remove(item.Tags, tagList);
                    _itemService.Save(item);
                }

                return Json(OK);
            }
            catch
            {
                return Json(FAIL);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public JsonResult ChangeCollectionName(string[] items, string newCollectionName)
        {
            try
            {
                if(items == null || items.Length == 0) return Json(OK);
                string collectionName = (newCollectionName ?? "").Trim();

                foreach(var id in items)
                {
                    ObjectId tid;
                    if(!ObjectId.TryParse(id, out tid))
                        continue;

                    var item = _itemService.FindOne(tid);
                    if(item == null) continue;
                    item.CollectionName = collectionName;
                    _itemService.Save(item);
                }

                return Json(OK);
            }
            catch
            {
                return Json(FAIL);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Delete(string[] items)
        {
            try
            {
                if(items == null || items.Length == 0)
                    return Json(OK);

                foreach(var id in items)
                {
                    ObjectId tid;
                    if(!ObjectId.TryParse(id, out tid))
                        continue;

                    _itemService.Delete(tid);
                }

                return Json(OK);
            }
            catch
            {
                return Json(FAIL);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Share(string[] items, string share, string groupName)
        {
            try
            {
                var ui = HttpContext.User.Identity as UserIdentity;

                if(ui == null) return Json(FAIL);

                var user = _userService.FindOne(ui.UserId);

                if(user == null) return Json(FAIL);

                if(items == null || items.Length == 0)
                {
                    return Json(new
                                    {
                                        result = OK,
                                        updates = new Dictionary<string, ShareResultModel>()
                                    });
                }

                var group = _groupService.FindOneByName(groupName);

                if(group == null)
                {
                    return Json(FAIL);
                }

                var membership = (_groupService.FindGroupMembership(new[] { group.GroupId })).FirstOrDefault();

                if(membership == null ||
                    !(new[] { GroupMembershipType.Owner, GroupMembershipType.Moderator, GroupMembershipType.Member }.Contains(membership.Type))
                )
                {
                    return Json(FAIL);
                }

                Dictionary<string, ShareResultModel> shared = new Dictionary<string, ShareResultModel>();

                foreach(var id in items)
                {
                    ObjectId tid;
                    if(!ObjectId.TryParse(id, out tid))
                        continue;

                    var item = _itemService.FindOne(tid);
                    if(item == null) continue;

                    var systemLanguage = _systemLanguageService.FindOne(item.SystemLanguageId);

                    var gsn = new GroupShareNotice()
                                  {
                                      UserId = user.UserId,
                                      Username = user.Fullname,
                                      LanguageName = systemLanguage.Code == SystemLanguage.NotYetSetCode ? "unknown" : systemLanguage.Name,
                                      ItemId = item.ItemId,
                                      Title = item.Title,
                                      CollectionName = item.CollectionName,
                                      GroupId = group.GroupId,
                                      ItemType = item.ItemType
                                  };

                    var gids = new List<ObjectId>(item.GroupId);
                    if(share.Equals("share"))
                    {
                        if(gids.Contains(group.GroupId))
                            continue;

                        gids.Add(group.GroupId);
                        gsn.ShareDirection = ShareDirection.Share;
                    }
                    else
                    {
                        if(!gids.Contains(group.GroupId))
                            continue;

                        gids.Remove(group.GroupId);
                        gsn.ShareDirection = ShareDirection.Unshare;
                    }

                    item.GroupId = gids.Distinct().ToArray();
                    shared[item.ItemId.ToString()] = new ShareResultModel()
                                                         {
                                                             IsShared = item.GroupId.Length > 0,
                                                             SharedGroups = string.Join(", ", _groupService.FindAll(item.GroupId).Select(x => x.Name))
                                                         };
                    _itemService.Save(item);
                    _groupService.Save(gsn);
                }

                return Json
                    (
                    new
                    {
                        result = OK,
                        updates = shared.ToList()
                    }
                    );
            }
            catch
            {
                return Json(FAIL);
            }
        }
    }
}
