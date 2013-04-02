#region License
// AdminController.cs is part of ReadingTool.Site
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

using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using ReadingTool.Entities;
using ReadingTool.Repository;
using ReadingTool.Site.Attributes;
using ReadingTool.Site.Helpers;
using ReadingTool.Site.Models.Admin;

namespace ReadingTool.Site.Controllers.Home
{
    [CustomAuthorize(Roles = "ADMIN")]
    [NeedsPersistence]
    public class AdminController : Controller
    {
        private readonly Repository<SystemLanguage> _systemLanguageRepository;
        private log4net.ILog _logger = log4net.LogManager.GetLogger("Admin");

        public AdminController(Repository<SystemLanguage> systemLanguageRepository)
        {
            _systemLanguageRepository = systemLanguageRepository;
        }

        public ActionResult Index()
        {
            return View();
        }

        [AutoMap(typeof(IEnumerable<SystemLanguage>), typeof(IEnumerable<SystemLanguageIndexModel>))]
        public ActionResult Languages()
        {
            return View(_systemLanguageRepository.FindAll().OrderBy(x => x.Name));
        }

        [HttpGet]
        public ActionResult AddLanguage()
        {
            return View();
        }

        private void RefreshLanguageCache()
        {
            var languages = _systemLanguageRepository.FindAll().ToDictionary(x => x.Code, x => x.Name);

            HttpRuntime.Cache.Remove(MvcApplication.SYSTEM_LANGUAGE_CACHE_KEY);
            HttpRuntime.Cache.Add(
                MvcApplication.SYSTEM_LANGUAGE_CACHE_KEY,
                languages,
                null,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable,
                (key, value, reason) =>
                {
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddLanguage(SystemLanguageModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            SystemLanguage sl = new SystemLanguage()
                {
                    Code = model.Code,
                    Name = model.Name
                };

            _systemLanguageRepository.Save(sl);
            _logger.InfoFormat("New Language {0}/{1} added", sl.Name, sl.Code);

            this.FlashSuccess("System language added.");

            RefreshLanguageCache();

            return RedirectToAction("Languages");
        }

        [HttpGet]
        public ActionResult EditLanguage(int id)
        {
            var sl = _systemLanguageRepository.FindOne(id);

            if(sl == null)
            {
                return RedirectToAction("Languages");
            }

            return View(new SystemLanguageModel
                {
                    Code = sl.Code,
                    Name = sl.Name,
                    SystemLanguageId = sl.SystemLanguageId
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditLanguage(SystemLanguageModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            var sl = _systemLanguageRepository.FindOne(model.SystemLanguageId);
            _logger.InfoFormat("Edit Language from {0}/{1} to {2}/{3}", sl.Name, sl.Code, model.Name, model.Code);

            sl.Code = model.Code;
            sl.Name = model.Name;
            _systemLanguageRepository.Save(sl);

            this.FlashSuccess("System language updated.");

            RefreshLanguageCache();

            return RedirectToAction("Languages");
        }
    }
}
