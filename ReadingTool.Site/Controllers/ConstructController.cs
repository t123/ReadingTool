#if DEBUG
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using ReadingTool.Common;
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

            var user = _userService.CreateUser("travis", "travis");
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

            return Redirect("/");
        }
    }
}
#endif