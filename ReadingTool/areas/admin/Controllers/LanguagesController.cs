#region License
// LanguagesController.cs is part of ReadingTool
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
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using MvcContrib;
using ReadingTool.Areas.Admin.Models;
using ReadingTool.Attributes;
using ReadingTool.Entities;
using ReadingTool.Extensions;
using ReadingTool.Filters;
using ReadingTool.Services;

namespace ReadingTool.Areas.Admin.Controllers
{
    [CustomAuthorize(Roles = Entities.User.AllowedRoles.ADMIN)]
    public class LanguagesController : BaseController
    {
        private readonly ISystemLanguageService _systemLanguageService;

        public LanguagesController(ISystemLanguageService systemLanguageService)
        {
            _systemLanguageService = systemLanguageService;
        }

        [AutoMap(typeof(IEnumerable<SystemLanguage>), typeof(IEnumerable<SystemLanguageModel>))]
        public ActionResult Index()
        {
            return View(_systemLanguageService.FindAllInUse());
        }

        [HttpGet]
        public ActionResult Import()
        {
            return View(new ImportLanguageModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Import(ImportLanguageModel model)
        {
            if(ModelState.IsValid)
            {
                IList<SystemLanguage> languages = new List<SystemLanguage>();

                string csv;
                var currentLanguages = _systemLanguageService.FindAll().ToDictionary(x => x.Code);
                using(TextReader tr = new StreamReader(model.File.InputStream, Encoding.UTF8))
                {
                    csv = tr.ReadToEnd();
                }

                int i = 0;
                foreach(string line in csv.Split('\n'))
                {
                    string[] split = line.Split('\t');

                    if(i++ == 0)
                    {
                        try
                        {
                            var ccode = split[model.CodeColumnNo];
                            var cname = split[model.LanguageNameColumnNo];

                            if(ccode != "Id" || cname != "Ref_Name")
                                throw new Exception("Are you sure this is the right file?");
                        }
                        catch
                        {
                            throw new Exception("Are you sure this is the right file?");
                        }
                        continue;
                    }

                    string code = split[model.CodeColumnNo];
                    if(currentLanguages.ContainsKey(code)) continue;

                    languages.Add(new SystemLanguage()
                                      {
                                          Code = code,
                                          Name = split[model.LanguageNameColumnNo]
                                      });
                }

                if(_systemLanguageService.FindByCode(SystemLanguage.NotYetSetCode) == null)
                {
                    languages.Add(new SystemLanguage()
                                      {
                                          Code = SystemLanguage.NotYetSetCode,
                                          Name = "Not yet set"
                                      });
                }

                _systemLanguageService.Save(languages);
                return this.RedirectToAction(x => x.Import()).Success("Languages imported");
            }

            return View(model).Error("Languages not imported");
        }
    }
}
