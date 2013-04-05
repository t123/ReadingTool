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
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
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
using ReadingTool.Site.Models.Account;
using ReadingTool.Site.Models.Groups;
using ReadingTool.Site.Models.Languages;
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
        private readonly Repository<Term> _termRepository;
        private readonly Repository<Language> _languageRepository;
        private readonly IGroupService _groupService;
        private readonly IParserService _parserService;
        private readonly ITextService _textService;

        public GroupsController(
            Repository<User> userRepository,
            Repository<Group> groupRepository,
            Repository<Text> textRepository,
            Repository<GroupMembership> membershipRepository,
            Repository<Term> termRepository,
            Repository<Language> languageRepository,
            IGroupService groupService,
            IParserService parserService,
            ITextService textService
            )
        {
            _userRepository = userRepository;
            _groupRepository = groupRepository;
            _textRepository = textRepository;
            _membershipRepository = membershipRepository;
            _termRepository = termRepository;
            _languageRepository = languageRepository;
            _groupService = groupService;
            _parserService = parserService;
            _textService = textService;
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
                         .Any(y => y.User == _userRepository.LoadOne(UserId) && y.MembershipType == MembershipType.Owner || y.MembershipType == MembershipType.Moderator || y.MembershipType == MembershipType.Member || y.MembershipType == MembershipType.Invited)
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
            this.FlashSuccess("Group added.");

            return RedirectToAction("Edit", new { id = group.GroupId });
        }

        [HttpGet]
        public ActionResult Edit(Guid id)
        {
            var group = _groupService.HasAccess(id, UserId, new[] { MembershipType.Owner });

            if(group == null)
            {
                this.FlashError("Group not found.");
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

            if(group == null)
            {
                this.FlashError("Group not found.");
                return RedirectToAction("Index");
            }

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

            var texts = group.Texts.AsQueryable();

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

            foreach(var r in searchResult.Results)
            {
                r.GroupId = groupId;
            }

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
                this.FlashError("Group not found.");
                return RedirectToAction("Index");
            }

            var model = new GroupMembershipViewModel()
                {
                    GroupId = group.GroupId,
                    Name = group.Name,
                    GroupType = group.GroupType,
                    Members = group.Members.Select(x => new GroupMembershipModel
                        {
                            GroupId = group.GroupId,
                            UserId = x.User.UserId,
                            GroupMembershipId = x.GroupMembershipId,
                            GroupName = group.Name,
                            MembershipType = x.MembershipType,
                            Username = x.User.DisplayName
                        }
                        ).ToList(),
                };

            model.MembershipType = model.Members.FirstOrDefault(x => x.UserId == UserId).MembershipType;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Membership(Guid id, FormCollection form, string invites)
        {
            //TODO fix me

            var group = _groupService.HasAccess(id, UserId, new[] { MembershipType.Owner, MembershipType.Moderator });

            if(group == null)
            {
                this.FlashError("Group not found.");
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

                Guid removeTexts = Guid.Empty;
                if(value.Equals("delete", StringComparison.InvariantCultureIgnoreCase))
                {
                    var membership = group.Members.FirstOrDefault(x => x.GroupMembershipId == membershipId);
                    group.Members.Remove(membership);
                    removeTexts = membership.User.UserId;
                }
                else
                {
                    var member = group.Members.FirstOrDefault(x => x.GroupMembershipId == membershipId);

                    if(member == null)
                    {
                        continue;
                    }

                    member.MembershipType = (MembershipType)Enum.Parse(typeof(MembershipType), value);

                    if(
                        member.MembershipType == MembershipType.Pending ||
                        member.MembershipType == MembershipType.Banned ||
                        member.MembershipType == MembershipType.Invited
                        )
                    {
                        removeTexts = member.User.UserId;
                    }
                }

                if(removeTexts != Guid.Empty)
                {
                    var texts = group.Texts.Where(x => x.User.UserId == removeTexts).ToArray();

                    foreach(var t in texts)
                    {
                        group.Texts.Remove(t);
                    }
                }
            }

            foreach(var username in (invites ?? "").Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries))
            {
                var user = _userRepository.FindOne(x => x.Username == username);

                if(user == null)
                {
                    continue;
                }

                var member = group.Members.FirstOrDefault(x => x.User == user);

                if(member == null)
                {
                    group.Members.Add(new GroupMembership()
                        {
                            Group = group,
                            MembershipType = MembershipType.Invited,
                            User = user
                        });
                }
            }

            _groupRepository.Save(group);
            this.FlashSuccess("Group updated.");

            return RedirectToAction("Membership", new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LeaveGroup(Guid id)
        {
            var group = _groupService.HasAccess(id, UserId, new[] { MembershipType.Member, MembershipType.Moderator, MembershipType.Invited, });

            if(group == null)
            {
                this.FlashError("Group not found.");
                return RedirectToAction("Index");
            }

            var membership = group.Members.FirstOrDefault(x => x.User.UserId == UserId);
            group.Members.Remove(membership);
            var texts = group.Texts.Where(x => x.User.UserId == UserId).ToArray();
            foreach(var t in texts)
            {
                group.Texts.Remove(t);
            }

            _groupRepository.Save(group);
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
            var group = _groupRepository.FindOne(x => x.GroupId == id);

            if(group == null)
            {
                this.FlashError("Group not found.");
                return RedirectToAction("Browse");
            }

            var membership = group.Members.FirstOrDefault(x => x.User.UserId == UserId);

            if(group.GroupType == GroupType.Private && (membership == null || membership.MembershipType != MembershipType.Invited))
            {
                this.FlashError("Group not found.");
                return RedirectToAction("Browse");
            }

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

                this.FlashSuccess("A request has been submitted to the group moderators.");
                return RedirectToAction("Browse");
            }
            else
            {
                if(membership.MembershipType == MembershipType.Banned)
                {
                    this.FlashError("Group not found.");
                }
                else if(membership.MembershipType == MembershipType.Pending)
                {
                    this.FlashInfo("You already have a pending request to join this group.");
                }
                else if(membership.MembershipType == MembershipType.Invited)
                {
                    membership.MembershipType = MembershipType.Member;
                    this.FlashInfo("You have now joined this group.");
                    return RedirectToAction("Index");
                }

                return RedirectToAction("Browse");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UnshareTexts(Guid groupId, string texts)
        {
            var group = _groupService.HasAccess(groupId, UserId);

            if(group != null && !string.IsNullOrEmpty(texts))
            {
                foreach(var id in texts.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => Guid.Parse(x)))
                {
                    if(id == Guid.Empty)
                    {
                        continue;
                    }

                    var text = _textRepository.FindOne(x => x.TextId == id && x.User == _userRepository.LoadOne(UserId));

                    if(text != null && group.Texts.Contains(text))
                    {
                        group.Texts.Remove(text);
                    }
                }

                _groupRepository.Save(group);
            }

            this.FlashSuccess("Texts unshared");
            return RedirectToAction("Details", new { id = groupId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteGroup(Guid id)
        {
            var group = _groupService.HasAccess(id, UserId, new MembershipType[] { MembershipType.Owner, });

            if(group == null)
            {
                return RedirectToAction("Index");
            }

            group.Members.Clear();
            group.Texts.Clear();
            _groupRepository.Save(group);
            _groupRepository.Delete(group);

            this.FlashSuccess("Group deleted");
            return RedirectToAction("Index");
        }


        public ActionResult Read(Guid id, Guid groupId)
        {
            var text = _textService.FindOne(id, groupId, UserId);
            var group = _groupService.HasAccess(groupId, UserId);

            if(text == null || group == null)
            {
                return RedirectToAction("Index");
            }

            var model = Create(group, text, false);

            if(model == null)
            {
                this.FlashInfo("Sorry, the text could not be parsed. Do you have the language {0} in your language list?", text.Language1.Code);
                return RedirectToAction("Details", new { id = groupId });
            }

            return View("Read", model);
        }

        public ActionResult ReadParallel(Guid id, Guid groupId)
        {
            var text = _textService.FindOne(id, groupId, UserId);
            var group = _groupService.HasAccess(groupId, UserId);

            if(text == null || group == null)
            {
                return RedirectToAction("Index");
            }

            var model = Create(group, text, true);

            if(model == null)
            {
                this.FlashInfo("Sorry, the text could not be parsed. Do you have the language {0} in your language list?", text.Language1.Code);
                return RedirectToAction("Details", new { id = groupId });
            }

            return View("Read", model);
        }

        //public FileContentResult DownloadLatex(Guid id)
        //{
        //    var text = _textService.FindOne(id);

        //    if(text == null)
        //    {
        //        throw new FileNotFoundException();
        //    }

        //    var terms = _termRepository.FindAll(x => x.Language == text.Language1 && x.User == _userRepository.LoadOne(UserId)).ToArray();
        //    var latexParser = DependencyResolver.Current.GetService<LatexParserService>();
        //    var parsed = latexParser.Parse(false, text.Language1, text.Language2, terms, text);

        //    return new FileContentResult(Encoding.UTF8.GetBytes(parsed), "application/x-latex")
        //    {
        //        FileDownloadName = Path.Combine(text.Title, ".tex")
        //    };
        //}

        private ReadModel Create(Group group, Text text, bool asParallel)
        {
            if(asParallel && text.Language2 == null)
            {
                asParallel = false;
            }

            var language1 = _languageRepository.FindOne(x => x.Code == text.Language1.Code && x.User == _userRepository.LoadOne(UserId));
            var language2 = asParallel && text.Language2 != null ? _languageRepository.FindOne(x => x.Code == text.Language2.Code && x.User == _userRepository.LoadOne(UserId)) : null;

            if(language1 == null)
            {
                return null;
            }

            if(asParallel && language2 == null)
            {
                return null;
            }

            var terms = _termRepository.FindAll(x => x.Language.Code == language1.Code && x.User == _userRepository.LoadOne(UserId)).ToArray();
            var parsed = _parserService.Parse(asParallel, language1, language2, terms, text);

            Guid nextText = group.Texts.Where(x => x.Language1 == text.Language1 && x.CollectionName == text.CollectionName && x.CollectionNo > text.CollectionNo).OrderBy(x => x.CollectionNo).Select(x => x.TextId).FirstOrDefault();
            Guid previousText = group.Texts.Where(x => x.Language1 == text.Language1 && x.CollectionName == text.CollectionName && x.CollectionNo < text.CollectionNo).OrderByDescending(x => x.CollectionNo).Select(x => x.TextId).FirstOrDefault();

            return new ReadModel()
            {
                AsParallel = asParallel,
                ParsedText = parsed,
                Text = Mapper.Map<Text, TextViewModel>(text),
                Language = Mapper.Map<Language, LanguageViewModel>(language1),
                Language2 = !asParallel || text.Language2 == null ? null : Mapper.Map<Language, LanguageViewModel>(language2),
                User = Mapper.Map<User, UserModel>(_userRepository.FindOne(UserId)),
                PagedTexts = new Tuple<Guid, Guid>(previousText, nextText),
                ApiDomain = ConfigurationManager.AppSettings["ApiDomain"],
                Group = Mapper.Map<Group, GroupViewModel>(group)
            };
        }

        [AjaxRoute]
        [HttpPost]
        public ActionResult AutoUsernames(string query)
        {
            query = (query ?? "").Trim();
            dynamic response = new
                {
                    query = query,
                    suggestions = _userRepository
                        .FindAll(x => x.Username.StartsWith(query))
                        .OrderBy(x => x.Username)
                        .Take(10)
                        .ToList()
                        .Select(x => x.Username)
                };

            return new JsonNetResult() { Data = response };
        }
    }
}
