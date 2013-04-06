#region License
// LanguagesController.cs is part of ReadingTool.Site
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
using System.Web.Mvc;
using AutoMapper;
using ReadingTool.Common;
using ReadingTool.Common.Search;
using ReadingTool.Entities;
using ReadingTool.Repository;
using ReadingTool.Site.Attributes;
using ReadingTool.Site.Helpers;
using ReadingTool.Site.Models.Languages;

namespace ReadingTool.Site.Controllers.Home
{
    [Authorize]
    [NeedsPersistence]
    public class LanguagesController : BaseController
    {
        private readonly Repository<Language> _languageRepository;
        private readonly Repository<SystemLanguage> _systemLanguageRepository;
        private readonly Repository<User> _userRepository;
        private log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public LanguagesController(
            Repository<Language> languageRepository,
            Repository<SystemLanguage> systemLanguageRepository,
            Repository<User> userRepository
            )
        {
            _languageRepository = languageRepository;
            _systemLanguageRepository = systemLanguageRepository;
            _userRepository = userRepository;
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

            var languages = _languageRepository.FindAll(x => x.User == _userRepository.LoadOne(UserId));

            var count = languages.Count();
            switch(so.Sort)
            {
                default:
                    if(so.Direction == GridSortDirection.Asc)
                    {
                        languages = languages.OrderBy(x => x.Name);
                    }
                    else
                    {
                        languages = languages.OrderByDescending(x => x.Name);
                    }
                    break;
            }

            languages = languages.Skip(so.Skip).Take(so.RowsPerPage);

            var searchResult = new SearchResult<LanguageViewModel>()
            {
                Results = Mapper.Map<IEnumerable<Language>, IEnumerable<LanguageViewModel>>(languages),
                TotalRows = count
            };

            var result = new SearchGridResult<LanguageViewModel>()
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
            return View(new LanguageModel()
                {
                    RegexSplitSentences = @".!?:;",
                    RegexWordCharacters = @"a-zA-ZÀ-ÖØ-öø-ȳ",
                    Languages = _systemLanguageRepository.FindAll(x => x.Code != "xx").OrderBy(x => x.Name).ToDictionary(x => x.Code, x => x.Name),
                    ShowSpaces = true,
                    Modal = false,
                    ModalBehaviour = ModalBehaviour.LeftClick,
                    AutoPause = true
                });
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Add(LanguageModel model)
        {
            var exists = _languageRepository.FindOne(x => x.Code == model.Code && x.User == _userRepository.LoadOne(UserId));

            if(exists != null)
            {
                ModelState.AddModelError("Code", "You already have this language in your list.");
            }

            if(!ModelState.IsValid)
            {
                model.Languages = _systemLanguageRepository.FindAll(x => x.Code != "xx").OrderBy(x => x.Name).ToDictionary(x => x.Code, x => x.Name);
                return View(model);
            }

            Language l = new Language()
                {
                    Name = model.Name,
                    Code = model.Code,
                    Settings = new Language.LanguageSettings()
                        {
                            Direction = model.Direction,
                            RegexSplitSentences = model.RegexSplitSentences,
                            RegexWordCharacters = model.RegexWordCharacters,
                            ShowSpaces = model.ShowSpaces,
                            Modal = model.Modal,
                            ModalBehaviour = model.ModalBehaviour,
                            AutoPause = model.AutoPause
                        },
                    User = _userRepository.LoadOne(UserId)
                };

            _languageRepository.Save(l);
            this.FlashSuccess("Language added.");

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(Guid id)
        {
            var language = _languageRepository.FindOne(x => x.LanguageId == id && x.User.UserId == UserId);

            if(language == null)
            {
                return RedirectToAction("Index");
            }

            var model = new LanguageEditModel()
                {
                    Language = new LanguageModel
                        {
                            Code = language.Code,
                            Direction = language.Settings.Direction,
                            LanguageId = language.LanguageId,
                            Languages = _systemLanguageRepository.FindAll(x => x.Code != "xx").OrderBy(x => x.Name).ToDictionary(x => x.Code, x => x.Name),
                            Name = language.Name,
                            RegexSplitSentences = language.Settings.RegexSplitSentences,
                            RegexWordCharacters = language.Settings.RegexWordCharacters,
                            ShowSpaces = language.Settings.ShowSpaces,
                            Modal = language.Settings.Modal,
                            ModalBehaviour = language.Settings.ModalBehaviour,
                            AutoPause = language.Settings.AutoPause
                        }
                };

            model.Dictionaries = Mapper.Map<IList<UserDictionary>, IList<DictionaryViewModel>>(language.Dictionaries);

            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(Guid id, [Bind(Prefix = "Language")]LanguageModel model)
        {
            var exists = _languageRepository.FindOne(x => x.Code == model.Code && x.User == _userRepository.LoadOne(UserId) && x.LanguageId != model.LanguageId);

            if(exists != null)
            {
                ModelState.AddModelError("Code", "You already have this language in your list.");
            }

            if(!ModelState.IsValid)
            {
                var l = _languageRepository.FindOne(x => x.LanguageId == id && x.User.UserId == UserId);
                model.Languages = _systemLanguageRepository.FindAll(x => x.Code != "xx").OrderBy(x => x.Name).ToDictionary(x => x.Code, x => x.Name);

                LanguageEditModel led = new LanguageEditModel()
                    {
                        Dictionaries = Mapper.Map<IList<UserDictionary>, IList<DictionaryViewModel>>(l.Dictionaries),
                        Language = model,
                    };

                return View(led);
            }

            var language = _languageRepository.FindOne(x => x.LanguageId == id && x.User.UserId == UserId);

            if(language == null || id != model.LanguageId)
            {
                return RedirectToAction("Index");
            }

            language.Name = model.Name;
            language.Code = model.Code;
            language.Settings = new Language.LanguageSettings()
                {
                    Direction = model.Direction,
                    RegexSplitSentences = model.RegexSplitSentences,
                    RegexWordCharacters = model.RegexWordCharacters,
                    ShowSpaces = model.ShowSpaces,
                    Modal = model.Modal,
                    ModalBehaviour = model.ModalBehaviour,
                    AutoPause = model.AutoPause
                };

            _languageRepository.Save(language);
            this.FlashSuccess("Language updated.");

            return RedirectToAction("Edit", new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Guid id)
        {
            var language = _languageRepository.FindOne(x => x.LanguageId == id && x.User.UserId == UserId);

            if(language != null)
            {
                _languageRepository.Delete(language);
            }
            this.FlashSuccess("Language deleted.");

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult AddDictionary(Guid id)
        {
            var language = _languageRepository.FindOne(x => x.LanguageId == id && x.User.UserId == UserId);

            if(language == null)
            {
                return RedirectToAction("Index");
            }

            var model = new DictionaryModel()
                {
                    LanguageId = id
                };

            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult AddDictionary(DictionaryModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            var language = _languageRepository.LoadOne(model.LanguageId);
            var dictionary = new UserDictionary()
                {
                    Language = language,
                    Encoding = model.Encoding,
                    Name = model.Name,
                    WindowName = model.WindowName,
                    Url = model.Url,
                    Sentence = model.Sentence,
                    AutoOpen = model.AutoOpen
                };

            language.Dictionaries.Add(dictionary);
            _languageRepository.Save(language);
            this.FlashSuccess("Dictionary added.");

            return RedirectToAction("Edit", new { id = model.LanguageId });
        }

        [HttpGet]
        public ActionResult EditDictionary(Guid id, Guid dictionaryId)
        {
            var language = _languageRepository.FindOne(x => x.LanguageId == id && x.User.UserId == UserId);

            if(language == null)
            {
                return RedirectToAction("Index");
            }

            var dictionary = language.Dictionaries.FirstOrDefault(x => x.DictionaryId == dictionaryId);

            if(dictionary == null)
            {
                return RedirectToAction("Edit", new { id = language.LanguageId });
            }

            var model = new DictionaryModel()
            {
                LanguageId = id,
                DictionaryId = dictionaryId,
                Encoding = dictionary.Encoding,
                Name = dictionary.Name,
                Url = dictionary.Url,
                Sentence = dictionary.Sentence,
                WindowName = dictionary.WindowName,
                AutoOpen = dictionary.AutoOpen
            };

            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditDictionary(DictionaryModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            var language = _languageRepository.FindOne(model.LanguageId);
            var dictionary = language.Dictionaries.FirstOrDefault(x => x.DictionaryId == model.DictionaryId);

            dictionary.Language = language;
            dictionary.Encoding = model.Encoding;
            dictionary.Name = model.Name;
            dictionary.WindowName = model.WindowName;
            dictionary.Url = model.Url;
            dictionary.Sentence = model.Sentence;
            dictionary.AutoOpen = model.AutoOpen;

            _languageRepository.Save(language);
            this.FlashSuccess("Dictionary updated.");

            return RedirectToAction("Edit", new { id = model.LanguageId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteDictionary(Guid id, Guid dictionaryId)
        {
            var language = _languageRepository.FindOne(x => x.LanguageId == id && x.User.UserId == UserId);

            if(language != null)
            {
                var dictionary = language.Dictionaries.FirstOrDefault(x => x.DictionaryId == dictionaryId);

                if(dictionary != null)
                {
                    language.Dictionaries.Remove(dictionary);
                    _languageRepository.Save(language);
                }
            }

            this.FlashSuccess("Dictionary deleted.");

            return RedirectToAction("Edit", new { id = id });
        }
    }
}
