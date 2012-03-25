#region License
// ProfilesController.cs is part of ReadingTool
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
using System.Web.Mvc;
using AutoMapper;
using ReadingTool.Common;
using ReadingTool.Common.Enums;
using ReadingTool.Entities;
using ReadingTool.Models.View.User;
using ReadingTool.Services;

namespace ReadingTool.Controllers
{
    public class ProfilesController : BaseController
    {
        protected readonly IUserService _userService;
        protected readonly ILanguageService _languageService;
        protected readonly IWordService _wordService;
        protected readonly ISystemLanguageService _systemLanguageService;

        public ProfilesController(
            IUserService userService,
            ILanguageService languageService,
            IWordService wordService,
            ISystemLanguageService systemLanguageService
            )
        {
            _userService = userService;
            _languageService = languageService;
            _wordService = wordService;
            _systemLanguageService = systemLanguageService;
        }

        public ActionResult Index(string username)
        {
            if(string.IsNullOrEmpty(username))
            {
                return View("special/notfound");
            }

            var user = _userService.FindOneByUsername(username);

            if(user == null)
            {
                //No such user
                return View("special/notfound");
            }

            if(HttpContext.User.Identity.IsAuthenticated && HttpContext.User.Identity.Name.Equals(username, StringComparison.InvariantCultureIgnoreCase))
            {
                ViewBag.Owner = true;
            }
            else
            {
                if(user.PublicProfile == null || user.PublicProfile.Availability == PublicProfileAvailability.Nobody)
                {
                    //User exists, but doesn't want a profile
                    return View("special/notavailable");
                }

                if(user.PublicProfile.Availability == PublicProfileAvailability.Users && !HttpContext.User.Identity.IsAuthenticated)
                {
                    //Only authenticated users
                    return View("special/login");
                }
            }

            var profile = Mapper.Map<User, PublicProfileViewModel>(user);
            profile.AboutMe = MarkdownHelper.Default().Transform(profile.AboutMe);

            if(profile.ShowStats)
            {
                
            }

            return View(profile);
        }
    }
}
