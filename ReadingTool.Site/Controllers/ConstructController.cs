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
            _languageRepository.DeleteAll(x => true);
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
                };

            foreach(var l in systemLanguages)
            {
                _systemLanguageRepository.Save(l);
            }

            var l1 = new Language()
                {
                    Code = "en",
                    Name = "English",
                    User = user,
                    Settings = new Language.LanguageSettings
                        {
                            Direction = LanguageDirection.LTR,
                            RegexSplitSentences = ".!?:;",
                            RegexWordCharacters = "a-zA-ZÀ-ÖØ-öø-ȳ",
                        }
                };

            var l2 = new Language()
                {
                    Code = "fr",
                    Name = "French",
                    User = user,
                    Settings = new Language.LanguageSettings
                        {
                            Direction = LanguageDirection.LTR,
                            RegexSplitSentences = ".!?:;",
                            RegexWordCharacters = "a-zA-ZÀ-ÖØ-öø-ȳ",
                        }
                };

            _languageRepository.Save(l1);
            _languageRepository.Save(l2);

            for(int i = 1; i < 100; i++)
            {
                Text t = new Text()
                    {
                        CollectionName = "Assimil",
                        CollectionNo = i,
                        Created = DateTime.Now,
                        Language1 = l1,
                        Language2 = l2,
                        Modified = DateTime.Now,
                        Title = "Lesson " + i,
                        User = user,
                    };

                _textRepository.Save(t);

                t = new Text()
                {
                    CollectionName = "Teach Yourself",
                    CollectionNo = i,
                    Created = DateTime.Now,
                    Language1 = l2,
                    Language2 = null,
                    Modified = DateTime.Now,
                    Title = "Lesson " + i,
                    User = user,
                    LastRead = DateTime.Now
                };

                var term = new Term()
                    {
                        BasePhrase = "Base Term" + i,
                        Created = DateTime.Now,
                        User = user,
                        Text = t,
                        Modified = DateTime.Now,
                        Language = l1,
                        Definition = "def",
                        Phrase = "Term" + i,
                        Sentence = "Sentence" + i,
                        State = TermState.Known,
                    };

                _textRepository.Save(t);
                _termRepository.Save(term);
            }

            return Redirect("/");
        }
    }
}
#endif