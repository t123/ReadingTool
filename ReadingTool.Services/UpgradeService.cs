using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Principal;
using MongoDB.Bson;
using Newtonsoft.Json;
using ReadingTool.Core;
using ReadingTool.Core.Enums;
using ReadingTool.Entities;

namespace ReadingTool.Services
{
    public interface IUpgradeService
    {
        void Upgrade(string jsonString);
    }

    public class UpgradeService : IUpgradeService
    {
        private readonly ITextService _textService;
        private readonly ITermService _termService;
        private readonly ILanguageService _languageService;
        private readonly ISystemLanguageService _systemLanguageService;
        private readonly IUserIdentity _identity;

        public UpgradeService(
            IPrincipal principal,
            ITextService textService,
            ITermService termService,
            ILanguageService languageService,
            ISystemLanguageService systemLanguageService
            )
        {
            _textService = textService;
            _termService = termService;
            _languageService = languageService;
            _systemLanguageService = systemLanguageService;
            _identity = principal.Identity as IUserIdentity;
        }

        public void Upgrade(string jsonString)
        {
            dynamic json = JsonConvert.DeserializeObject(jsonString);

            var lmap = new Dictionary<string, ObjectId>();
            var tmap = new Dictionary<string, ObjectId?>();

            foreach(var language in json.Languages)
            {
                var l = new Language()
                    {
                        Name = language.Name,
                        Owner = _identity.UserId,
                        Colour = language.Colour,
                        Settings = new LanguageSettings()
                            {
                                CharacterSubstitutions = @"´='|`='|’='|‘='|...=…|..=‥|»=|«=|“=|”=|„=|‟=|""=",
                                RegexWordCharacters = @"a-zA-ZÀ-ÖØ-öø-ȳ",
                                RegexSplitSentences = ".!?:;",
                                ExceptionSplitSentences = "[A-Z].|Dr.",
                                KeepFocus = true,
                                Direction = LanguageDirection.LTR,
                                ModalBehaviour = ModalBehaviour.LeftClick,
                                RemoveSpaces = false,
                                SplitEachCharacter = false,
                                ShowRomanisation = false
                            }
                    };

                foreach(var d in language.Dictionaries)
                {
                    l.Dictionaries.Add(new LanguageDictionary()
                        {
                            AutoOpen = false,
                            DisplayOrder = 1,
                            Id = ObjectId.GenerateNewId(),
                            Name = d.Name,
                            Url = d.Url,
                            Parameter = DictionaryParameter.Word,
                            UrlEncoding = "",
                            WindowName = d.WindowName
                        }
                        );
                }

                _languageService.Save(l);
                lmap[language.LanguageId.ToString()] = l.Id;
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
                        Owner = _identity.UserId,
                        Metadata = new TextMetadata() { LastSeen = text.LastSeen },
                        Tags = new string[] { }
                    };

                string lid = text.LanguageId.ToString();
                t.L1Id = lmap.GetValueOrDefault(lid, ObjectId.Empty);

                if(text.Tags != null)
                {
                    List<string> tags = new List<string>();
                    foreach(string t1 in text.Tags)
                    {
                        tags.Add(t1);
                    }
                    t.Tags = tags.ToArray();
                }

                _textService.Save(t);
                tmap[text.ItemId.ToString()] = t.Id;
            }

            var terms = new List<Term>();
            foreach(var word in json.Words)
            {
                Term t = new Term()
                    {
                        Id = ObjectId.GenerateNewId(),
                        Owner = _identity.UserId,
                        Box = word.Box,
                        Length = word.Length,
                        State = (TermState)word.State,
                        TermPhrase = word.WordPhrase
                    };

                DateTime? nextReview = word.NextReview;
                if(nextReview.HasValue && nextReview.Value.Year < 2000)
                {
                    t.NextReview = null;
                }
                else
                {
                    t.NextReview = nextReview.Value;
                }

                string lid = word.LanguageId.ToString();
                t.LanguageId = lmap.GetValueOrDefault(lid, ObjectId.Empty);

                var it = new IndividualTerm()
                    {
                        BaseTerm = word.BaseWord,
                        Romanisation = word.Romanisation,
                        Sentence = word.Sentence,
                        Definition = word.Definition,
                    };

                string tid = word.ItemId.ToString();
                it.TextId = tmap.GetValueOrDefault(tid, (ObjectId?)null);

                if(word.Tags != null)
                {
                    List<string> tags = new List<string>();
                    foreach(string t1 in word.Tags)
                    {
                        tags.Add(t1);
                    }
                    it.Tags = tags.ToArray();
                }

                t.IndividualTerms.Add(it);
                terms.Add(t);
            }

            _termService.SaveAll(terms, false);
        }
    }
}
