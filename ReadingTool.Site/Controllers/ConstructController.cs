#region License
// ConstructController.cs is part of ReadingTool.Site
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
#if DEBUG
using System;
using System.Web.Mvc;
using ReadingTool.Entities;
using ReadingTool.Repository;
using ReadingTool.Services;
using ReadingTool.Site.Attributes;

namespace ReadingTool.Site.Controllers.Home
{
    [NeedsPersistence]
    public class ConstructController : Controller
    {
        private readonly Repository<User> _userRepository;
        private readonly Repository<SystemLanguage> _systemLanguageRepository;
        private readonly IUserService _userService;
        private readonly Repository<Language> _languageRepository;
        private readonly Repository<Text> _textRepository;
        private readonly Repository<Term> _termRepository;

        public ConstructController(
            Repository<User> userRepository,
            Repository<SystemLanguage> systemLanguageRepository,
            IUserService userService,
            Repository<Language> languageRepository,
            Repository<Text> textRepository,
            Repository<Term> termRepository
            )
        {
            _userRepository = userRepository;
            _systemLanguageRepository = systemLanguageRepository;
            _userService = userService;
            _languageRepository = languageRepository;
            _textRepository = textRepository;
            _termRepository = termRepository;
        }

        public ActionResult Index()
        {
            _systemLanguageRepository.DeleteAll(x => true);
            //_languageRepository.DeleteAll(x => true);
            _userRepository.DeleteAll(x => true);

            var user = _userService.CreateUser("admin", "admin");
            if(user == null)
            {
                throw new Exception("Could not construct user");
            }

            var systemLanguages = new[] 
                {
                    new SystemLanguage { Code = "en", Name = "English"},
                    new SystemLanguage { Code = "fr", Name = "French"},
                    new SystemLanguage { Code = "lv", Name = "Latvian"},
                    new SystemLanguage { Code = "de", Name = "German"},
                    new SystemLanguage { Code = "sw", Name = "Swedish"},
                    new SystemLanguage { Code = "tr", Name = "Turkish"},
                    new SystemLanguage { Code = "pl", Name = "Polish"},
                };

            foreach(var l in systemLanguages)
            {
                _systemLanguageRepository.Save(l);
            }

            return RedirectToAction("Index", "~~Home");
        }
    }
}
#endif