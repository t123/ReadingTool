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
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using MvcContrib;
using ReadingTool.Attributes;
using ReadingTool.Common;
using ReadingTool.Entities;
using ReadingTool.Extensions;
using ReadingTool.Filters;
using ReadingTool.Models.Create.Language;
using ReadingTool.Services;

namespace ReadingTool.Controllers
{
    [CustomAuthorize]
    public class LanguagesController : BaseController
    {
        private readonly ILanguageService _languageService;
        private readonly ISystemLanguageService _systemLanguageService;
        private readonly Dictionary<string, string> _colours = new Dictionary<string, string> 
        {
            { "#FB4C2F", "red"},
            { "#FFAD46", "light orange"},
            { "#FF7537", "dark orange"},
            { "#B6CFF5", "light blue"},
            { "#4986E7", "dark blue"},
            { "#A2DCC1", "light green"},
            { "#16A765", "dark green"},
            { "#E3D7FF", "light purple"},
            { "#B99AFF", "dark purple"},
            { "#E7E7E7", "light grey"},
        };

        public LanguagesController(ILanguageService languageService, ISystemLanguageService systemLanguageService)
        {
            _languageService = languageService;
            _systemLanguageService = systemLanguageService;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Add()
        {
            ViewBag.Colours = _colours;
            return View(new LanguageModel()); //Leave the model, need the constructor defaults
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(LanguageModel model)
        {
            if(string.IsNullOrEmpty(model.SystemLanguageName))
            {
                ModelState.AddModelError("SystemLanguageName", "Please type in a language name");
            }
            else if(_systemLanguageService.FindByName(model.SystemLanguageName) == null)
            {
                ModelState.AddModelError("SystemLanguageName", string.Format("{0} is not a valid language", model.SystemLanguageName));
            }

            if(ModelState.IsValid)
            {
                var language = Mapper.Map<LanguageModel, Language>(model);
                _languageService.Save(language);

                return this.RedirectToAction(x => x.Edit(language.LanguageId.ToString())).Success("Language added");
            }

            ViewBag.Colours = _colours;
            return View(model).Error(Messages.FormValidationError);
        }

        [HttpGet]
        [AutoMap(typeof(Language), typeof(LanguageModel))]
        public ActionResult Edit(string id)
        {
            var language = _languageService.FindOne(id);

            if(language == null)
            {
                return this.RedirectToAction(x => x.Index()).Error("Language not found");
            }

            ViewBag.Dictionaries = Mapper.Map<IList<UserDictionary>, IList<UserDictionaryModel>>(language.Dictionaries);
            ViewBag.Colours = _colours;

            return View(language);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string id, LanguageModel model)
        {
            if(string.IsNullOrEmpty(model.SystemLanguageName))
            {
                ModelState.AddModelError("SystemLanguageName", "Please type in a language name");
            }
            else if(_systemLanguageService.FindByName(model.SystemLanguageName) == null)
            {
                ModelState.AddModelError("SystemLanguageName", string.Format("{0} is not a valid language", model.SystemLanguageName));
            }

            var language = _languageService.FindOne(id);

            if(ModelState.IsValid)
            {
                TryUpdateModel(language, new[]
                                             {
                                                 "Name", "Colour", "TranslateUrl", "KeepFocus", "DefaultMediaUrl", 
                                                 "IsRtlLanguage", "HasRomanisationField", "ModalBehaviour",
                                                 "DefaultDictionary", "RemoveSpaces", "PunctuationRegEx", "SentenceEndRegEx"
                                             });

                language.Punctuation = model.Punctuation.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                language.SystemLanguageId = _systemLanguageService.FindByName(model.SystemLanguageName).SystemLanguageId;
                _languageService.Save(language);

                return this.RedirectToAction(x => x.Edit(language.LanguageId.ToString())).Success("Language saved");
            }

            ViewBag.Colours = _colours;
            ViewBag.Dictionaries = Mapper.Map<IList<UserDictionary>, IList<UserDictionaryModel>>(language.Dictionaries);

            return View(model).Error(Messages.FormValidationError);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string id)
        {
            _languageService.Delete(id);
            return this.RedirectToAction(x => x.Index()).Success("Language deleted");
        }

        [HttpGet]
        public ActionResult AddDictionary(string id)
        {
            var language = _languageService.FindOne(id);

            if(language == null)
            {
                return this.RedirectToAction(x => x.Index()).Error("Language not found");
            }

            ViewBag.LanguageId = id;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddDictionary(string id, UserDictionaryModel model)
        {
            if(ModelState.IsValid)
            {
                var language = _languageService.FindOne(id);
                var dictionary = Mapper.Map<UserDictionaryModel, UserDictionary>(model);
                language.Dictionaries.Add(dictionary);
                _languageService.Save(language);

                return this.RedirectToAction(x => x.Edit(id)).Success("Dictionary added");
            }

            ViewBag.LanguageId = id;
            return View(model).Error(Messages.FormValidationError);
        }

        [HttpGet]
        [AutoMap(typeof(UserDictionary), typeof(UserDictionaryModel))]
        public ActionResult EditDictionary(string id, string name)
        {
            var language = _languageService.FindOne(id);

            if(language == null)
            {
                return this.RedirectToAction(x => x.Index()).Error("Language not found");
            }

            ViewBag.LanguageId = id;
            var dictionary = language.Dictionaries.FirstOrDefault(x => x.Name == name);

            if(dictionary == null)
            {
                return this.RedirectToAction(x => x.Edit(id)).Error("Dictionary not found");
            }

            return View(dictionary);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditDictionary(string id, string originalname, UserDictionaryModel model)
        {
            var language = _languageService.FindOne(id);

            if(ModelState.IsValid)
            {
                var dictionary = Mapper.Map<UserDictionaryModel, UserDictionary>(model);
                _languageService.UpdateDictionary(language, originalname, dictionary);
                return this.RedirectToAction(x => x.EditDictionary(id, dictionary.Name)).Success("Dictionary saved");
            }

            ViewBag.LanguageId = id;
            return View(model).Error(Messages.FormValidationError);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteDictionary(string id, string name)
        {
            var language = _languageService.FindOne(id);

            if(language == null)
            {
                return this.RedirectToAction(x => x.Index()).Error("Language not found");
            }

            _languageService.DeleteDictionary(language, name);
            return this.RedirectToAction(x => x.Edit(id)).Success("Dictionary deleted");
        }
    }
}
