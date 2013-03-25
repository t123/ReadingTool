using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using ReadingTool.Common;
using ReadingTool.Entities;
using ReadingTool.Repository;
using ReadingTool.Services;
using ReadingTool.Site.Attributes;

namespace ReadingTool.Site.Controllers.Home
{
    [Authorize]
    [NeedsPersistence]
    public class UpgradeController : Controller
    {
        private Guid UserId
        {
            get { return Guid.Parse(HttpContext.User.Identity.Name); }
        }

        private readonly Repository<User> _userRepository;
        private readonly Repository<Term> _termRepository;
        private readonly Repository<Text> _textRepository;
        private readonly Repository<Language> _languageRepository;
        private readonly Repository<Tag> _tagRepository;
        private readonly ITextService _textService;

        public UpgradeController(
            Repository<User> userRepository,
            Repository<Term> termRepository,
            Repository<Text> textRepository,
            Repository<Language> languageRepository,
            Repository<Tag> tagRepository,
            ITextService textService
            )
        {
            _userRepository = userRepository;
            _termRepository = termRepository;
            _textRepository = textRepository;
            _languageRepository = languageRepository;
            _tagRepository = tagRepository;
            _textService = textService;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Upgrade()
        {
            string jsonString;
            using(StreamReader sr = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "account.json"), Encoding.UTF8))
            {
                jsonString = sr.ReadToEnd();
            }

            dynamic json = JsonConvert.DeserializeObject(jsonString);

            var lmap = new Dictionary<string, Guid>();
            var tmap = new Dictionary<string, Guid?>();

            foreach(var language in json.Languages)
            {
                var l = new Language()
                {
                    Name = language.Name,
                    User = _userRepository.LoadOne(UserId),
                    Settings = new Language.LanguageSettings()
                    {
                        RegexWordCharacters = @"a-zA-ZÀ-ÖØ-öø-ȳ",
                        RegexSplitSentences = ".!?:;",
                        Direction = LanguageDirection.LTR,
                        ShowSpaces = true,
                    }
                };

                switch(l.Name.ToLowerInvariant())
                {
                    case "french":
                        l.Code = "fr";
                        l.Settings = new Language.LanguageSettings()
                            {
                                RegexWordCharacters = @"a-zA-ZÀ-ÖØ-öø-ȳ\-\'",
                                RegexSplitSentences = ".!?:;",
                                Direction = LanguageDirection.LTR,
                                ShowSpaces = true,
                            };
                        break;

                    case "german":
                        l.Code = "de";
                        break;

                    case "latvian":
                        l.Code = "lv";
                        break;

                    case "polish":
                        l.Code = "pl";
                        break;

                    case "swedish":
                        l.Code = "sw";
                        break;

                    case "turkish":
                        l.Code = "tr";
                        break;

                    default:
                        l.Code = "en";
                        break;
                }

                if(!string.IsNullOrEmpty(language.TranslateUrl.ToString()))
                {
                    l.Dictionaries.Add(new UserDictionary()
                        {
                            Name = "Translate",
                            AutoOpen = false,
                            Encoding = null,
                            Language = l,
                            Sentence = true,
                            Url = language.TranslateUrl.ToString().Replace("[[text]]", "###"),
                            WindowName = "gtrans"
                        });
                }

                foreach(var d in language.Dictionaries)
                {
                    l.Dictionaries.Add(new UserDictionary()
                        {
                            Name = d.Name,
                            Url = (d.Url.ToString() ?? "").Replace("[[word]]", "###"),
                            Sentence = false,
                            Encoding = d.Encoding,
                            WindowName = d.WindowName,
                            Language = l,
                            AutoOpen = language.DefaultDictionary.ToString() == d.Name.ToString()
                        }
                        );
                }

                _languageRepository.Save(l);
                lmap[language.LanguageId.ToString()] = l.LanguageId;
            }

            foreach(var text in json.Items)
            {
                Text t = new Text()
                {
                    CollectionName = text.CollectionName,
                    CollectionNo = text.CollectionNo,
                    Title = text.Title,
                    AudioUrl = text.Url,
                    L1Text = text.L1Text,
                    L2Text = text.L2Text,
                    User = _userRepository.LoadOne(UserId),
                    LastRead = text.LastSeen,
                    Created = text.Created,
                    Modified = text.Modified
                };

                string lid = text.LanguageId.ToString();
                t.Language1 = _languageRepository.FindOne(lmap.GetValueOrDefault(lid, Guid.Empty));

                if(!string.IsNullOrEmpty(t.L2Text))
                {
                    t.Language2 = t.Language1;
                }

                _textService.Save(t);
                tmap[text.ItemId.ToString()] = t.TextId;
            }

            var allTags = _tagRepository.FindAll().ToList();

            foreach(var word in json.Words)
            {
                Term t = new Term()
                {
                    User = _userRepository.LoadOne(UserId),
                    Box = word.Box,
                    Length = word.Length,
                    State = (TermState)word.State,
                    Phrase = word.WordPhrase,
                    BasePhrase = word.BaseWord,
                    Sentence = word.Sentence,
                    Definition = word.Definition,
                    Created = word.Created,
                    Modified = word.Modified
                };

                if(t.State == TermState.Ignore)
                {
                    continue;
                }

                DateTime? nextReview = word.NextReview;
                if(t.State != TermState.NotKnown)
                {
                    t.NextReview = null;
                }
                else if(nextReview.HasValue && nextReview.Value.Year < 2000)
                {
                    t.NextReview = null;
                }
                else
                {
                    t.NextReview = nextReview.Value;
                }

                string lid = word.LanguageId.ToString();
                t.Language = _languageRepository.FindOne(lmap.GetValueOrDefault(lid, Guid.Empty));

                string tid = word.ItemId.ToString();
                t.Text = _textRepository.FindOne(tmap.GetValueOrDefault(tid, (Guid?)null));

                if(word.Tags != null)
                {
                    foreach(var tag in word.Tags)
                    {
                        var existing = allTags.FirstOrDefault(x => x.TagTerm == tag.ToString());
                        if(existing == null)
                        {
                            existing = new Tag()
                                {
                                    TagTerm = tag.ToString()
                                };

                            allTags.Add(existing);
                        }

                        t.Tags.Add(existing);
                    }
                }

                //t.HasTags = t.Tags.Count > 0;

                Regex regex = new Regex(@"([" + t.Language.Settings.RegexWordCharacters + @"])");
                if(!regex.IsMatch(t.Phrase))
                {
                    t.State = TermState.Ignore;
                }

                _termRepository.Save(t);
            }

            return RedirectToAction("Index");
        }
    }
}
