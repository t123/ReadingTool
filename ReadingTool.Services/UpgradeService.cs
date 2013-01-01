using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Principal;
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
        private readonly IDbConnection _db;
        private readonly ITextService _textService;
        private readonly ITermService _termService;
        private readonly ILanguageService _languageService;
        private readonly ISystemLanguageService _systemLanguageService;
        private readonly IUserIdentity _identity;

        public UpgradeService(
            IDbConnection db,
            IPrincipal principal,
            ITextService textService,
            ITermService termService,
            ILanguageService languageService,
            ISystemLanguageService systemLanguageService
            )
        {
            _db = db;
            _textService = textService;
            _termService = termService;
            _languageService = languageService;
            _systemLanguageService = systemLanguageService;
            _identity = principal.Identity as IUserIdentity;
        }

        public void Upgrade(string jsonString)
        {
            dynamic json = JsonConvert.DeserializeObject(jsonString);

            var lmap = new Dictionary<string, Guid>();
            var tmap = new Dictionary<string, Guid?>();

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
                    l.AddDictionary(new LanguageDictionary()
                        {
                            AutoOpen = false,
                            DisplayOrder = 1,
                            Id = SequentialGuid.NewGuid(),
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
                        LastSeen = text.LastSeen,
                        Tags = ""
                    };

                string lid = text.LanguageId.ToString();
                t.L1Id = lmap.GetValueOrDefault(lid, Guid.Empty);

                if(text.Tags != null)
                {
                    foreach(var tag in text.Tags)
                    {
                        t.Tags += ((string)tag).ToLowerInvariant().Trim() + " ";
                    }
                }

                _textService.Save(t);
                tmap[text.ItemId.ToString()] = t.Id;
            }

            var terms = new List<Term>();
            foreach(var word in json.Words)
            {
                Term t = new Term()
                    {
                        Id = SequentialGuid.NewGuid(),
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
                t.LanguageId = lmap.GetValueOrDefault(lid, Guid.Empty);

                var it = new IndividualTerm()
                    {
                        BaseTerm = word.BaseWord,
                        Romanisation = word.Romanisation,
                        Sentence = word.Sentence,
                        TermId = t.Id,
                        Definition = word.Definition,
                    };

                string tid = word.ItemId.ToString();
                it.TextId = tmap.GetValueOrDefault(tid, (Guid?)null);

                if(word.Tags != null)
                {
                    foreach(var tag in word.Tags)
                    {
                        it.Tags += ((string)tag).ToLowerInvariant().Trim() + " ";
                    }
                }

                t.AddIndividualTerm(it);
                terms.Add(t);
            }

            _termService.SaveAll(terms, false);
        }
    }
}
