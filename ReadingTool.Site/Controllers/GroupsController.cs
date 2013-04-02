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
            var group = _groupService.HasAccess(id, UserId);

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

            var group = _groupService.HasAccess(model.GroupId, UserId);
            group.Name = model.Name.Trim();
            group.Description = model.Description.Trim();
            group.GroupType = model.GroupType;
            _groupRepository.Save(group);

            return RedirectToAction("Edit", new { id = model.GroupId });
        }

        public ActionResult Details(Guid id)
        {
            return View();
        }

        public PartialViewResult DetailsGrid(string sort, GridSortDirection sortDir, int? page, string filter, int? perPage)
        {
            var so = new SearchOptions()
            {
                Filter = filter,
                Page = page ?? 1,
                RowsPerPage = perPage ?? SearchGridPaging.DefaultRows,
                Sort = sort ?? "language",
                Direction = sortDir
            };

            var texts = _textRepository.FindAll(x => x.User == _userRepository.LoadOne(UserId));
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

            var searchResult = new SearchResult<TextViewModel>()
            {
                Results = Mapper.Map<IEnumerable<Text>, IEnumerable<TextViewModel>>(texts),
                TotalRows = count
            };

            var result = new SearchGridResult<TextViewModel>()
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
    }
}
