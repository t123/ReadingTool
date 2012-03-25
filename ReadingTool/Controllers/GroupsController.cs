#region License
// GroupsController.cs is part of ReadingTool
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
using AutoMapper;
using MongoDB.Bson;
using MvcContrib;
using ReadingTool.Attributes;
using ReadingTool.Common;
using ReadingTool.Common.Enums;
using ReadingTool.Common.Exceptions;
using ReadingTool.Common.Helpers;
using ReadingTool.Entities;
using ReadingTool.Entities.Parser;
using ReadingTool.Extensions;
using ReadingTool.Filters;
using ReadingTool.Models.Create.Group;
using ReadingTool.Models.View.Group;
using ReadingTool.Services;

namespace ReadingTool.Controllers
{
    [CustomAuthorize]
    public class GroupsController : BaseController
    {
        protected readonly IGroupService _groupService;
        protected readonly IUserService _userService;
        private readonly IParserService _parserService;
        private readonly ILanguageService _languageService;
        private readonly IItemService _itemService;

        public GroupsController(
            IGroupService groupService,
            IUserService userService,
            IParserService parserService,
            ILanguageService languageService,
            IItemService itemService
            )
        {
            _groupService = groupService;
            _userService = userService;
            _parserService = parserService;
            _languageService = languageService;
            _itemService = itemService;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add(GroupModel model)
        {
            if(ModelState.IsValid)
            {
                var group = Mapper.Map<GroupModel, Group>(model);

                _groupService.Save(group);
                return this.RedirectToAction(x => x.Index()).Success("Group added");
            }

            return View(model).Error(Messages.FormValidationError);
        }

        [HttpGet]
        [AutoMap(typeof(Group), typeof(GroupModel))]
        public ActionResult Edit(string id)
        {
            var group = _groupService.FindOneForUser(id, new[] { GroupMembershipType.Moderator, GroupMembershipType.Owner });

            if(group == null)
            {
                return this.RedirectToAction(x => x.Index()).Error("You do not have permission to edit this group");
            }

            return View(group);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(string id, GroupModel model)
        {
            IList<ObjectId> invites = new List<ObjectId>();

            if(!string.IsNullOrEmpty(model.Invitations))
            {
                foreach(var name in model.Invitations.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Distinct(StringComparer.InvariantCultureIgnoreCase))
                {
                    var user = _userService.FindOneByUsername(name);
                    if(user == null)
                    {
                        ModelState.AddModelError("Invitations", string.Format("{0} is not a valid username", name));
                    }
                    else
                    {
                        invites.Add(user.UserId);
                    }
                }
            }

            if(ModelState.IsValid)
            {
                var group = _groupService.FindOneForUser(id, new[] { GroupMembershipType.Moderator, GroupMembershipType.Owner });
                group.Name = model.Name;
                group.Tags = TagHelper.Split(model.Tags);
                group.About = model.About;
                group.Type = model.Type;
                _groupService.Save(group);
                return this.RedirectToAction(x => x.Edit(id)).Success("Group saved");
            }

            return View(model).Error(Messages.FormValidationError);
        }

        [HttpGet]
        public ActionResult Manage(string id)
        {
            var group = _groupService.FindOneForUser(id, new[] { GroupMembershipType.Moderator, GroupMembershipType.Owner });

            if(group == null)
            {
                return this.RedirectToAction(x => x.Index()).Error("You do not have permission to manage this group");
            }

            return View((object)id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(string id, GroupModel model)
        {
            //TODO clean this up
            var keys = Request.Form.AllKeys;
            var members = _groupService.FindAllMembers(new ObjectId(id), new[] { "" }); //FIXME sending {""} to get all groups

            var current = members.FirstOrDefault(x => x.UserId == UserId);

            if(current == null || (current.Type != GroupMembershipType.Owner && current.Type != GroupMembershipType.Moderator))
            {
                return this.RedirectToAction(x => x.Index()).Error("You do not have permission to manage this group");
            }

            foreach(var key in keys)
            {
                ObjectId userId;

                if(!ObjectId.TryParse(key, out userId))
                {
                    continue;
                }

                _groupService.ChangeMembership(members.FirstOrDefault(x => x.UserId == userId), Request.Form[key]);
            }

            return this.RedirectToAction(x => x.Manage(id)).Success("Group updated");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddUsers(string id)
        {
            //TODO clean this up
            var group = _groupService.FindOneForUser(id, new[] { GroupMembershipType.Moderator, GroupMembershipType.Owner });

            if(group == null)
            {
                return this.RedirectToAction(x => x.Index()).Error("You do not have permission to manage this group");
            }

            var usernames = (Request["Usernames"] ?? "").Split(new[] { TagHelper.TAG_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
            var members = _groupService.FindAllMembers(@group.GroupId, new[] { "" }); //FIXME see manageusers as well
            var newMembers = new List<GroupMember>();

            foreach(var username in usernames)
            {
                var user = _userService.FindOneByUsername(username);

                if(user == null)
                    continue;

                if(members.Any(x => x.UserId == user.UserId))
                    continue;

                newMembers.Add(new GroupMember
                {
                    GroupId = group.GroupId,
                    Type = GroupMembershipType.Invitation,
                    UserId = user.UserId
                });
            }

            if(newMembers.Count > 0)
            {
                _groupService.AddGroupMember(newMembers);
            }

            return this.RedirectToAction(x => x.Manage(id)).Success("Users invited");
        }

        [HttpGet]
        [AutoMap(typeof(Group), typeof(GroupModel))]
        public ActionResult View(string id)
        {
            var group = _groupService.FindOneForUser(id, new[] { GroupMembershipType.Moderator, GroupMembershipType.Owner, GroupMembershipType.Member });

            if(group == null)
            {
                return this.RedirectToAction(x => x.Index()).Error("You do not have permission to view this group");
            }

            return View(group);
        }

        [HttpGet]
        [AutoMap(typeof(Group), typeof(GroupViewModel))]
        public ActionResult ViewInfo(string id)
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

            if(group == null)
            {
                return this.RedirectToAction(x => x.Index()).Error("You do not have permission to view this group");
            }

            group.About = MarkdownHelper.Default().Transform(group.About);

            var membership = Mapper.Map<GroupMember, GroupMemberViewModel>(_groupService.GetMembership(group.GroupId));
            ViewBag.Membership = membership;

            return View(group);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult JoinGroup(string id)
        {
            var group = _groupService.FindOne(id);

            var membership = _groupService.GetMembership(new ObjectId(id));

            if(membership == null)
            {
                if(group.Type == GroupType.InvitationOnly)
                {
                    return this.RedirectToAction(x => x.ViewInfo(id)).Success("Could not join group");
                }
                else
                {
                    _groupService.Save(new GroupMember { GroupId = group.GroupId, UserId = UserId, Type = GroupMembershipType.Request });
                    return this.RedirectToAction(x => x.ViewInfo(id)).Success("A request has been sent for you to join this group");
                }
            }
            else
            {
                if(membership.Type == GroupMembershipType.Banned)
                {
                    return this.RedirectToAction(x => x.ViewInfo(id)).Success("You have been banned from this group");
                }
                else
                {
                    membership.Type = GroupMembershipType.Member;
                    _groupService.Save(membership);
                    return this.RedirectToAction(x => x.ViewInfo(id)).Success("You have joined this group");
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LeaveGroup(string id)
        {
            var membership = _groupService.GetMembership(new ObjectId(id));

            if(membership == null)
            {
                return this.RedirectToAction(x => x.ViewInfo(id)).Success("Could not leave group");
            }

            _groupService.DeleteMembership(membership);

            return this.RedirectToAction(x => x.Index()).Success("You have left this group");
        }

        private Tuple<ObjectId, string> GetEquivalentUserLanguage(ObjectId systemLanguageId)
        {
            var userLanguage = _languageService.FindOneForUserBySystemLanguage(UserId, systemLanguageId);

            if(userLanguage == null)
            {
                return new Tuple<ObjectId, string>(ObjectId.Empty, "You do not have this language in your list of languages");
            }

            return new Tuple<ObjectId, string>(userLanguage.LanguageId, string.Empty);
        }

        public ActionResult Read(string id, string groupId)
        {
            ObjectId textId;
            ObjectId gId;
            if(!ObjectId.TryParse(id, out textId)) return new HttpNotFoundResult(id + " does not exists on this server, try add http:// to the front of your file.");
            if(!ObjectId.TryParse(groupId, out gId)) return new HttpNotFoundResult(id + " does not exists on this server, try add http:// to the front of your file.");

            var item = _itemService.FindOneForGroup(textId, gId);

            if(item == null)
            {
                return this.RedirectToAction(x => x.View(groupId)).Error("Text not found");
            }

            ObjectId languageId;

            if(item.Owner == UserId)
            {
                languageId = item.LanguageId;
            }
            else
            {
                var result = GetEquivalentUserLanguage(item.SystemLanguageId);

                if(result.Item1 == ObjectId.Empty)
                    return this.RedirectToAction(x => x.View(groupId)).Error(result.Item2);

                languageId = result.Item1;
            }

            var wordService = DependencyResolver.Current.GetService<IWordService>();
            var userService = DependencyResolver.Current.GetService<IUserService>();
            var parserInput = new ParserInput
                (
                    userService.FindOne(UserId),
                    _languageService.FindOne(languageId),
                    item,
                    wordService.FindAllForParsingSplit(languageId),
                    false
                );

            try
            {
                var parserOutput = _parserService.Parse(parserInput);
                ViewBag.GroupId = groupId;

                if(!item.ShareUrl)
                {
                    parserOutput.Item.Url = string.Empty;
                }

                return View(parserOutput);
            }
            catch(TooManyWords e)
            {
                return View("toomanywords", e);
            }
        }

        public ActionResult ReadParallel(string id, string groupId)
        {
            ObjectId textId;
            ObjectId gId;
            if(!ObjectId.TryParse(id, out textId)) return new HttpNotFoundResult(id + " does not exists on this server, try add http:// to the front of your file.");
            if(!ObjectId.TryParse(groupId, out gId)) return new HttpNotFoundResult(id + " does not exists on this server, try add http:// to the front of your file.");

            var item = _itemService.FindOneForGroup(textId, gId);

            if(item == null)
            {
                return this.RedirectToAction(x => x.View(groupId)).Error("Item not found");
            }

            if(!item.IsParallel)
            {
                return this.RedirectToAction(x => x.View(groupId)).Error("This is not a parallel text");
            }

            ObjectId languageId;

            if(item.Owner == UserId)
            {
                languageId = item.LanguageId;
            }
            else
            {
                var result = GetEquivalentUserLanguage(item.SystemLanguageId);

                if(result.Item1 == ObjectId.Empty)
                    return this.RedirectToAction(x => x.View(groupId)).Error(result.Item2);

                languageId = result.Item1;
            }

            var wordService = DependencyResolver.Current.GetService<IWordService>();
            var userService = DependencyResolver.Current.GetService<IUserService>();
            var parserInput = new ParserInput
                (
                    userService.FindOne(UserId),
                    _languageService.FindOne(languageId),
                    item,
                    wordService.FindAllForParsingSplit(languageId),
                    true
                );

            try
            {
                var parserOutput = _parserService.Parse(parserInput);
                ViewBag.GroupId = groupId;

                if(!item.ShareUrl)
                {
                    parserOutput.Item.Url = string.Empty;
                }

                return View("read", parserOutput);
            }
            catch(TooManyWords e)
            {
                return View("toomanywords", e);
            }
        }

        public ActionResult Watch(string id, string groupId)
        {
            ObjectId videoId;
            ObjectId gId;
            if(!ObjectId.TryParse(id, out videoId)) return new HttpNotFoundResult(id + " does not exists on this server, try add http:// to the front of your file.");
            if(!ObjectId.TryParse(groupId, out gId)) return new HttpNotFoundResult(id + " does not exists on this server, try add http:// to the front of your file.");

            var item = _itemService.FindOneForGroup(videoId, gId);

            if(item == null)
            {
                return this.RedirectToAction(x => x.View(groupId)).Error("Video not found");
            }

            ObjectId languageId;

            if(item.Owner == UserId)
            {
                languageId = item.LanguageId;
            }
            else
            {
                var result = GetEquivalentUserLanguage(item.SystemLanguageId);

                if(result.Item1 == ObjectId.Empty)
                    return this.RedirectToAction(x => x.View(groupId)).Error(result.Item2);

                languageId = result.Item1;
            }

            var wordService = DependencyResolver.Current.GetService<IWordService>();
            var userService = DependencyResolver.Current.GetService<IUserService>();
            var parserInput = new ParserInput
                (
                    userService.FindOne(UserId),
                    _languageService.FindOne(languageId),
                    item,
                    wordService.FindAllForParsingSplit(languageId),
                    null
                );
            try
            {
                var parserOutput = _parserService.Parse(parserInput);

                ViewBag.GroupId = groupId;
                return View(parserOutput);
            }
            catch(TooManyWords e)
            {
                return View("toomanywords", e);
            }
        }

        [AutoMap(typeof(IEnumerable<GroupShareNotice>), typeof(IEnumerable<GroupShareNoticeViewModel>))]
        public ActionResult Rss(string id)
        {
            var group = _groupService.FindOneForUser(id, new[] { GroupMembershipType.Moderator, GroupMembershipType.Owner, GroupMembershipType.Member });

            if(group == null)
            {
                return this.RedirectToAction(x => x.Index()).Error("You do not have permission to view this group");
            }

            ViewBag.Group = Mapper.Map<Group, GroupViewModel>(group);
            var rssItems = _groupService.FindAllForRss(id);
            return View(rssItems);
        }
    }
}
