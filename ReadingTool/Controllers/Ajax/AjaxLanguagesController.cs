#region License
// AjaxLanguagesController.cs is part of ReadingTool
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
using System.Linq;
using System.Web.Mvc;
using ReadingTool.Attributes;
using ReadingTool.Services;

namespace ReadingTool.Controllers.Ajax
{
    [CustomAuthorize]
    public class AjaxLanguagesController : Controller
    {
        private const string OK = @"OK";
        private const string FAIL = @"FAIL";
        private const int LIMIT = 20;

        private readonly ISystemLanguageService _systemLanguageService;
        private readonly ILanguageService _languageService;

        public AjaxLanguagesController(
            ISystemLanguageService systemLanguageService,
            IWordService wordService,
            ILanguageService languageService,
            IUserService userService,
            IMessageService messageService,
            IGroupService groupService,
            IItemService itemService
            )
        {
            _systemLanguageService = systemLanguageService;
            _languageService = languageService;
        }

        public void Index()
        {
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Search()
        {
            var langauges = _languageService.FindAllForOwner();

            var filtered = langauges.Select(x => new
            {
                id = x.LanguageId.ToString(),
                name = x.Name,
                systemName = _systemLanguageService.FindOne(x.SystemLanguageId).Name,
                colour = x.Colour
            }
            ).OrderBy(x => x.name, StringComparer.InvariantCultureIgnoreCase);

            return Json(
                new
                {
                    items = filtered,
                    totalItems = filtered.Count(),
                    totalPages = (int)Math.Ceiling((double)filtered.Count() / (double)LIMIT)
                }
                );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteLanguage(string[] id)
        {
            if(id == null || id.Length == 0) return Json(OK);

            foreach(var lid in id)
            {
                _languageService.Delete(lid);
            }

            return Json(OK);
        }
    }
}
