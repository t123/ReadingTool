#region License
// RemoteValidatorController.cs is part of ReadingTool
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

using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using MongoDB.Bson;
using ReadingTool.Services;

namespace ReadingTool.Controllers
{
    [OutputCache(Location = OutputCacheLocation.None, NoStore = true)]
    public class RemoteValidatorController : Controller
    {
        private readonly IUserService _userService;
        private readonly IGroupService _groupService;
        private readonly ISystemLanguageService _systemLanguageService;

        private readonly string[] _usernames = new string[]
                                                   {
                                                       "admin",
                                                       "root",
                                                       "administrator",
                                                       "unknown",
                                                       "feedback",
                                                       "abuse",
                                                       "info",
                                                       "information"
                                                   };

        public RemoteValidatorController(IUserService userService, ISystemLanguageService systemLanguageService, IGroupService groupService)
        {
            _userService = userService;
            _groupService = groupService;
            _systemLanguageService = systemLanguageService;
        }

        [HttpPost]
        public JsonResult UsernameInUse(string username)
        {
            const string message = "This username has already been used";

            if(_usernames.Any(x => x == username.ToLowerInvariant())) return Json(message);

            var user = _userService.FindOneByUsername(username);

            if(user != null) return Json(message);
            return Json(true);
        }

        [HttpPost]
        public JsonResult GroupNameInUse(string name, string groupId)
        {
            const string message = "This name has already been used";

            ObjectId id;
            if(string.IsNullOrEmpty(groupId))
            {
                id = ObjectId.Empty;
            }
            else if(!ObjectId.TryParse(groupId, out id))
            {
                return Json(message);
            }

            var group = _groupService.FindOneByName(name, id);
            if(group != null) return Json(message);
            return Json(true);
        }

        [HttpPost]
        public JsonResult ValidateSystemLanguageName(string systemLanguageName)
        {
            const string message = "This language does not exist";

            if(_systemLanguageService.FindByName(systemLanguageName) == null)
                return Json(message);

            return Json(true);
        }

        [HttpPost]
        public JsonResult ValidateNativeLanguageName(string nativeLanguage)
        {
            const string message = "This language does not exist";

            if(_systemLanguageService.FindByName(nativeLanguage) == null)
                return Json(message);

            return Json(true);
        }
    }
}