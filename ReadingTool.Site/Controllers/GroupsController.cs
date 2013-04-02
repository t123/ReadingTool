#region License
// LanguageViewModel.cs is part of ReadingTool.Site
// 
// ReadingTool.Site is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Site is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Site. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2013 Travis Watt
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using ReadingTool.Common;
using ReadingTool.Common.Search;
using ReadingTool.Entities;
using ReadingTool.Repository;
using ReadingTool.Services;
using ReadingTool.Site.Attributes;
using ReadingTool.Site.Helpers;
using ReadingTool.Site.Models.Groups;
using ReadingTool.Site.Models.Texts;

namespace ReadingTool.Site.Controllers.Home
{
    [Authorize]
    [NeedsPersistence]
    public class GroupsController : BaseController
    {
        private readonly Repository<User> _userRepository;
        private readonly Repository<Group> _groupRepository;
        private readonly Repository<Text> _textRepository;
        private readonly Repository<GroupMembership> _membershipRepository;
        private readonly IGroupService _groupService;

        public GroupsController(
            Repository<User> userRepository,
            Repository<Group> groupRepository,
            Repository<Text> textRepository,
            Repository<GroupMembership> membershipRepository,
            IGroupService groupService
            )
        {
            _userRepository = userRepository;
            _groupRepository = groupRepository;
            _textRepository = textRepository;
            _membershipRepository = membershipRepository;
            _groupService = groupService;
        }

        public ActionResult Index()
        {
            return View();
        }

        public PartialViewResult IndexGrid(string sort, GridSortDirection sortDir, int? page, string filter, int? perPage)
        {
            var so = new SearchOptions()
            {
                Filter = filter,
                Page = page ?? 1,
                RowsPerPage = perPage ?? SearchGridPaging.DefaultRows,
                Sort = sort ?? "name",
                Direction = sortDir
            };

            var groups = _groupRepository.FindAll(
                x => x.Members.Select(y => new { y.User, y.MembershipType })
                         .Any(y => y.User == _userRepository.LoadOne(UserId) && y.MembershipType == MembershipType.Owner || y.MembershipType == MembershipType.Moderator || y.MembershipType == MembershipType.Member)
                );

            var count = groups.Count();
            switch(so.Sort)
            {
                default:
                    if(so.Direction == GridSortDirection.Asc)
                    {
                        groups = groups.OrderBy(x => x.Name);
                    }
                    else
                    {
                        groups = groups.OrderByDescending(x => x.Name);
                    }
                    break;
            }

            groups = groups.Skip(so.Skip).Take(so.RowsPerPage);

            //TODO fix me
            var searchResult = new SearchResult<GroupViewModel>()
            {
                Results = Mapper.Map<IEnumerable<Group>, IEnumerable<GroupViewModel>>(groups),
                TotalRows = count
            };

            foreach(var r in searchResult.Results)
            {
                r.MembershipType = _membershipRepository.FindOne(y => y.User == _userRepository.LoadOne(UserId) && y.Group == _groupRepository.LoadOne(r.GroupId)).MembershipType;
            }

            var result = new SearchGridResult<GroupViewModel>()
            {
                Items = searchResult.Results,
                Paging = new SearchGridPaging()
                {
                    Page = so.Page,
                    TotalRows = searchResult.TotalRows,
                    RowsPerPage = perPage ?? SearchGridPaging.DefaultRows
                },
                Direction = sortDir,
                Sort = sort
            };

            return PartialView("Partials/_grid", result);
        }

        [HttpGet]
        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(GroupModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            Group group = new Group()
                {
                    Name = model.Name.Trim(),
                    Description = model.Description.Trim(),
                    GroupType = model.GroupType,
                };

            group.Members = new List<GroupMembership>()
                {
                    new GroupMembership()
                        {
                            Group = group,
                            User = _userRepository.LoadOne(UserId),
                            MembershipType = MembershipType.Owner
                        }
                };

            _groupRepository.Save(group);
            this.FlashSuccess("Group added");

            return RedirectToAction("Edit", new { id = group.GroupId });
        }

        [HttpGet]
        public ActionResult Edit(Guid id)
        {
            var group = _groupService.HasAccess(id, UserId, new[] { MembershipType.Owner });

            if(group == null)
            {
                this.FlashError("Group not found");
                return RedirectToAction("Index");
            }

            var membership = group.Members.FirstOrDefault(x => x.User == _userRepository.LoadOne(UserId));

            var model = new GroupModel()
                {
                    GroupId = group.GroupId,
                    Description = group.Description,
                    Name = group.Name,
                    GroupType = group.GroupType,
                    CurrentUser = new GroupMembershipModel()
                        {
                            UserId = UserId,
                            GroupMembershipId = membership.GroupMembershipId,
                            MembershipType = membership.MembershipType,
                        }
                };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(GroupModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            var group = _groupService.HasAccess(model.GroupId, UserId, new[] { MembershipType.Owner });
            group.Name = model.Name.Trim();
            group.Description = model.Description.Trim();
            group.GroupType = model.GroupType;
            _groupRepository.Save(group);

            return RedirectToAction("Edit", new { id = model.GroupId });
        }

        public ActionResult Details(Guid id)
        {
            var group = _groupService.HasAccess(id, UserId);

            var model = new GroupViewModel()
                {
                    GroupId = group.GroupId,
                    Name = group.Name,
                    MembershipType = group.Members.First(x => x.User.UserId == UserId).MembershipType
                };

            return View(model);
        }

        public PartialViewResult DetailsGrid(string sort, GridSortDirection sortDir, int? page, string filter, int? perPage, string data)
        {
            var groupId = ServiceStack.Text.JsonSerializer.DeserializeFromString<Guid>(data);

            var group = _groupService.HasAccess(groupId, UserId);

            if(group == null)
            {
                throw new UnauthorizedAccessException();
            }

            var so = new SearchOptions()
            {
                Filter = filter,
                Page = page ?? 1,
                RowsPerPage = perPage ?? SearchGridPaging.DefaultRows,
                Sort = sort ?? "language",
                Direction = sortDir
            };

            var texts = group.Texts;

            var filterTerms = SearchFilterParser.Parse(filter);

            foreach(var term in filterTerms.Other)
            {
                texts = texts.Where(x => x.Title.Contains(term) || x.CollectionName.Contains(term));
            }

            var count = texts.Count();

            switch(so.Sort)
            {
                case "collectionname":
                    if(so.Direction == GridSortDirection.Asc)
                    {
                        texts = texts.OrderBy(x => x.CollectionName).ThenBy(x => x.CollectionNo).ThenBy(x => x.Title);
                    }
                    else
                    {
                        texts = texts.OrderByDescending(x => x.CollectionName).ThenBy(x => x.CollectionNo).ThenBy(x => x.Title);
                    }
                    break;

                case "lastread":
                    if(so.Direction == GridSortDirection.Asc)
                    {
                        texts = texts.OrderBy(x => x.LastRead);
                    }
                    else
                    {
                        texts = texts.OrderByDescending(x => x.LastRead);
                    }
                    break;

                case "title":
                    if(so.Direction == GridSortDirection.Asc)
                    {
                        texts = texts.OrderBy(x => x.Title);
                    }
                    else
                    {
                        texts = texts.OrderByDescending(x => x.Title);
                    }
                    break;

                default:
                    if(so.Direction == GridSortDirection.Asc)
                    {
                        texts = texts.OrderBy(x => x.Language1.Name).ThenBy(x => x.CollectionName).ThenBy(x => x.CollectionNo).ThenBy(x => x.Title);
                    }
                    else
                    {
                        texts = texts.OrderByDescending(x => x.Language1.Name).ThenBy(x => x.CollectionName).ThenBy(x => x.CollectionNo).ThenBy(x => x.Title);
                    }
                    break;
            }

            texts = texts.Skip(so.Skip).Take(so.RowsPerPage);

            var searchResult = new SearchResult<GroupTextViewModel>()
            {
                Results = Mapper.Map<IEnumerable<Text>, IEnumerable<GroupTextViewModel>>(texts),
                TotalRows = count
            };

            var result = new SearchGridResult<GroupTextViewModel>()
            {
                Items = searchResult.Results,
                Paging = new SearchGridPaging()
                {
                    Page = so.Page,
                    TotalRows = searchResult.TotalRows,
                    RowsPerPage = perPage ?? SearchGridPaging.DefaultRows
                },
                Direction = sortDir,
                Sort = sort
            };

            return PartialView("Partials/_detailsgrid", result);
        }

        [HttpGet]
        public ActionResult Membership(Guid id)
        {
            var group = _groupService.HasAccess(id, UserId, new[] { MembershipType.Owner, MembershipType.Moderator });

            if(group == null)
            {
                this.FlashError("Group not found");
                return RedirectToAction("Index");
            }

            var model = new GroupMembershipViewModel()
                {
                    GroupId = group.GroupId,
                    Name = group.Name,
                    Members = group.Members.Select(x => new GroupMembershipModel
                        {
                            GroupId = group.GroupId,
                            UserId = x.User.UserId,
                            GroupMembershipId = x.GroupMembershipId,
                            GroupName = group.Name,
                            MembershipType = x.MembershipType,
                            Username = x.User.DisplayName
                        }
                        ).ToList()
                };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Membership(Guid id, FormCollection form)
        {
            //TODO fix me

            var group = _groupService.HasAccess(id, UserId, new[] { MembershipType.Owner, MembershipType.Moderator });

            if(group == null)
            {
                this.FlashError("Group not found");
                return RedirectToAction("Index");
            }

            foreach(var key in form.AllKeys.Where(x => x.StartsWith("m_")))
            {
                Guid membershipId;
                string value;
                try
                {
                    Guid.TryParse(key.Substring(2, key.Length - 2), out membershipId);
                    value = form[key];
                }
                catch
                {
                    continue;
                }

                if(membershipId == Guid.Empty)
                {
                    continue;
                }

                if(value.Equals("delete", StringComparison.InvariantCultureIgnoreCase))
                {
                    var membership = group.Members.FirstOrDefault(x => x.GroupMembershipId == membershipId);
                    group.Members.Remove(membership);
                }
                else
                {
                    var member = group.Members.FirstOrDefault(x => x.GroupMembershipId == membershipId);

                    if(member == null)
                    {
                        continue;
                    }

                    member.MembershipType = (MembershipType)Enum.Parse(typeof(MembershipType), value);
                }
            }

            _groupRepository.Save(group);
            this.FlashSuccess("Group updated");

            return RedirectToAction("Membership", new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LeaveGroup(Guid id)
        {
            var group = _groupService.HasAccess(id, UserId, new[] { MembershipType.Member, MembershipType.Moderator });

            if(group == null)
            {
                this.FlashError("Group not found");
                return RedirectToAction("Index");
            }

            var membership = group.Members.FirstOrDefault(x => x.User.UserId == UserId);
            group.Members.Remove(membership);

            this.FlashSuccess("You have been removed from this group.");

            return RedirectToAction("Index");
        }

        public ActionResult Browse()
        {
            return View();
        }

        public PartialViewResult BrowseGrid(string sort, GridSortDirection sortDir, int? page, string filter, int? perPage)
        {
            var so = new SearchOptions()
            {
                Filter = filter,
                Page = page ?? 1,
                RowsPerPage = perPage ?? SearchGridPaging.DefaultRows,
                Sort = sort ?? "name",
                Direction = sortDir
            };

            var groups = _groupRepository.FindAll(
                x =>
                    x.GroupType == GroupType.Public &&
                    x.Members.All(y => y.User != _userRepository.LoadOne(UserId))
                );

            if(!string.IsNullOrEmpty(filter))
            {
                groups = groups.Where(x => x.Name.StartsWith(filter));
            }

            var count = groups.Count();
            switch(so.Sort)
            {
                default:
                    if(so.Direction == GridSortDirection.Asc)
                    {
                        groups = groups.OrderBy(x => x.Name);
                    }
                    else
                    {
                        groups = groups.OrderByDescending(x => x.Name);
                    }
                    break;
            }

            groups = groups.Skip(so.Skip).Take(so.RowsPerPage);

            var searchResult = new SearchResult<GroupViewModel>()
            {
                Results = Mapper.Map<IEnumerable<Group>, IEnumerable<GroupViewModel>>(groups),
                TotalRows = count
            };

            var result = new SearchGridResult<GroupViewModel>()
            {
                Items = searchResult.Results,
                Paging = new SearchGridPaging()
                {
                    Page = so.Page,
                    TotalRows = searchResult.TotalRows,
                    RowsPerPage = perPage ?? SearchGridPaging.DefaultRows
                },
                Direction = sortDir,
                Sort = sort
            };

            return PartialView("Partials/_browsegrid", result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult JoinGroup(Guid id)
        {
            var group = _groupRepository.FindOne(x => x.GroupId == id && x.GroupType == GroupType.Public);

            if(group == null)
            {
                this.FlashError("Group not found");
                return RedirectToAction("Browse");
            }

            var membership = group.Members.FirstOrDefault(x => x.User.UserId == UserId);

            if(membership == null)
            {
                var groupMembership = new GroupMembership()
                    {
                        Group = group,
                        MembershipType = MembershipType.Pending,
                        User = _userRepository.LoadOne(UserId)
                    };

                group.Members.Add(groupMembership);
                _groupRepository.Save(group);

                this.FlashSuccess("A request has been submitted to the groups moderators.");
                return RedirectToAction("Browse");
            }
            else
            {
                if(membership.MembershipType == MembershipType.Banned)
                {
                    this.FlashError("Group not found");
                }
                else if(membership.MembershipType == MembershipType.Pending)
                {
                    this.FlashInfo("You already have a pending request to join this group");
                }
                else
                {
                    this.FlashInfo("You are already a member of this group.");
                }

                return RedirectToAction("Browse");
            }
        }
    }
}
