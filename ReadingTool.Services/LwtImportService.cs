﻿#region License
// LwtImportService.cs is part of ReadingTool.Services
// 
// ReadingTool.Services is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Services is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Services. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2013 Travis Watt
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;
using NHibernate;
using Newtonsoft.Json;
using ReadingTool.Common;
using ReadingTool.Entities;
using ReadingTool.Repository;

namespace ReadingTool.Services
{
    public class LwtImportService
    {
        private readonly ITextService _textService;
        private readonly ISession _session;
        private readonly IPrincipal _principal;
        private readonly Repository<SystemLanguage> _systemLanguageRepository;
        private readonly Repository<Text> _textRepository;
        private readonly Repository<Term> _termRepository;
        private readonly Repository<Language> _languageRepository;
        private readonly Repository<User> _userRepository;
        private readonly Repository<Tag> _tagRepository;
        private JsonLwt _lwtData = new JsonLwt();
        private readonly UserIdentity _identity;

        public LwtImportService(
            ITextService textService,
            ISession session,
            IPrincipal principal,
            Repository<SystemLanguage> systemLanguageRepository,
            Repository<Text> textRepository,
            Repository<Term> termRepository,
            Repository<Language> languageRepository,
            Repository<User> userRepository,
            Repository<Tag> tagRepository
            )
        {
            _textService = textService;
            _session = session;
            _principal = principal;
            _systemLanguageRepository = systemLanguageRepository;
            _textRepository = textRepository;
            _termRepository = termRepository;
            _languageRepository = languageRepository;
            _userRepository = userRepository;
            _tagRepository = tagRepository;
            _identity = principal.Identity as UserIdentity;
        }

        private class JsonLwt
        {
            public IList<ArchivedTexts> LWT_ArchivedTexts { get; set; }
            public IList<ArchTextTags> LWT_ArchTextTags { get; set; }
            public IList<Languages> LWT_Languages { get; set; }
            public IList<Tags> LWT_Tags { get; set; }
            public IList<Tags2> LWT_Tags2 { get; set; }
            public IList<Texts> LWT_Texts { get; set; }
            public IList<TextTags> LWT_TextTags { get; set; }
            public IList<Words> LWT_Words { get; set; }
            public IList<WordTags> LWT_WordTags { get; set; }
        }

        public void Import(string json, bool testMode)
        {
            _lwtData = JsonConvert.DeserializeObject<JsonLwt>(json);
            string date = DateTime.Now.ToString("ddMM-HHmm");
            var systemLanguage = _systemLanguageRepository.FindOne(x => x.Code == "xx");
            var user = _userRepository.FindOne(_identity.UserId);

            //Regex regex = new Regex(model.NumberCollectionRegEx);

            foreach(var item in _lwtData.LWT_Languages)
            {
                Language l = new Language()
                {
                    Name = item.LgName + "-" + date,
                    Code = "xx",
                    User = user,
                    Settings = new Language.LanguageSettings()
                        {
                            AutoPause = true,
                            Direction = LanguageDirection.LTR,
                            Modal = true,
                            ModalUserBehaviour = ModalBehaviour.LeftClick,
                            ShowSpaces = item.LgRemoveSpaces.HasValue ? !item.LgRemoveSpaces.Value : false,
                            RegexWordCharacters = item.LgRegexpWordCharacters,
                            RegexSplitSentences = item.LgRegexpSplitSentences
                        }
                };

                if(!string.IsNullOrEmpty(item.LgDict1URI))
                {
                    l.Dictionaries.Add(
                        new UserDictionary()
                            {
                                Name = "Dictionary 1",
                                Url = item.LgDict1URI,
                                WindowName = "dict1",
                                AutoOpen = false,
                                Encoding = "",
                                Language = l,
                                Sentence = false
                            }
                        );
                }

                if(!string.IsNullOrEmpty(item.LgDict2URI))
                {
                    l.Dictionaries.Add(
                        new UserDictionary()
                        {
                            Name = "Dictionary 2",
                            Url = item.LgDict2URI,
                            WindowName = "dict2",
                            AutoOpen = false,
                            Encoding = "",
                            Language = l,
                            Sentence = false
                        }
                        );
                }

                if(!string.IsNullOrEmpty(item.LgGoogleTranslateURI))
                {
                    l.Dictionaries.Add(
                        new UserDictionary()
                        {
                            Name = "Translate",
                            Url = item.LgGoogleTranslateURI,
                            WindowName = "translate",
                            AutoOpen = false,
                            Encoding = "",
                            Language = l,
                            Sentence = true
                        }
                        );
                }

                if(!testMode)
                {
                    _languageRepository.Save(l);
                }

                item.Id = l.LanguageId;
            }

            foreach(var item in _lwtData.LWT_ArchivedTexts)
            {
                var lid = _lwtData.LWT_Languages.First(x => x.LgID == item.AtLgID);
                if(lid == null) throw new Exception("Could not find language with ID: " + item.AtLgID);

                var text = new Text()
                {
                    Language1 = _languageRepository.LoadOne(lid.Id),
                    Title = item.AtTitle,
                    L1Text = item.AtText,
                    User = user,
                    Created = DateTime.Now,
                    Modified = DateTime.Now,
                    L2Text = "",
                    AudioUrl = item.AtAudioURI,
                    LastRead = null,
                    CollectionName = "LWT Import",
                };

                if(!testMode)
                {
                    _textService.Save(text);
                }
            }

            foreach(var item in _lwtData.LWT_Texts)
            {
                var lid = _lwtData.LWT_Languages.First(x => x.LgID == item.TxLgID);
                if(lid == null) throw new Exception("Could not find language with ID: " + item.TxLgID);

                var text = new Text()
                {
                    Language1 = _languageRepository.LoadOne(lid.Id),
                    Title = item.TxTitle,
                    L1Text = item.TxText,
                    User = user,
                    Created = DateTime.Now,
                    Modified = DateTime.Now,
                    L2Text = "",
                    AudioUrl = item.TxAudioURI,
                    LastRead = null,
                    CollectionName = "LWT Import",
                };

                if(!testMode)
                {
                    _textService.Save(text);
                }
            }

            foreach(var item in _lwtData.LWT_Words)
            {
                List<string> atags = new List<string>();
                Term word = new Term()
                {
                    Created = item.WoCreated ?? DateTime.Now,
                    Modified = item.WoStatusChanged ?? DateTime.Now,
                    User = user,
                    Language = _languageRepository.LoadOne(_lwtData.LWT_Languages.First(x => x.LgID == item.WoLgID).Id),
                    Definition = item.WoTranslation ?? "",
                    Sentence = (item.WoSentence ?? "").Replace("{", "").Replace("}", ""),
                    Phrase = (item.WoText ?? "").Replace("{", "").Replace("}", ""),
                    BasePhrase = string.IsNullOrEmpty(item.WoRomanization) || item.WoRomanization == "*" ? "" : item.WoRomanization,
                };

                if((item.WoStatus ?? 1) == 98)
                {
                    word.Box = 1;
                    word.State = TermState.Ignore;
                }
                else if((item.WoStatus ?? 1) == 99)
                {
                    word.State = TermState.Known;
                    word.Box = 9;
                }
                else
                {
                    word.State = TermState.NotKnown;
                    word.Box = ((short?)item.WoStatus) ?? 1;

                    var nr = Term.NextReviewDate(word);

                    word.State = nr.Item1;
                    word.NextReview = nr.Item2;
                }

                var links = _lwtData.LWT_WordTags.Where(x => x.WtWoID == item.WoID);
                atags.AddRange(from i in links select _lwtData.LWT_Tags.FirstOrDefault(x => x.TgID == i.WtTgID) into t where t != null select t.TgText);

                foreach(var tag in atags)
                {
                    var existing = _tagRepository.FindOne(x => x.TagTerm.Equals(tag.ToLowerInvariant()));

                    if(existing == null)
                    {
                        existing = new Tag()
                        {
                            TagTerm = tag
                        };
                    }

                    word.Tags.Add(existing);
                }

                if(!testMode)
                {
                    _termRepository.Save(word);
                }
            }
        }

        #region classes
        private class ArchivedTexts
        {
            public int? AtId { get; set; }
            public int? AtLgID { get; set; }
            public string AtTitle { get; set; }
            public string AtText { get; set; }
            public string AtAudioURI { get; set; }
        }

        private class ArchTextTags
        {
            public int? AgAtID { get; set; }
            public int? AgT2ID { get; set; }
        }

        private class Languages
        {
            public Guid Id { get; set; }


            public int? LgID { get; set; }
            public string LgName { get; set; }
            public string LgDict1URI { get; set; }
            public string LgDict2URI { get; set; }
            public string LgGoogleTranslateURI { get; set; }
            public string LgGoogleTTSURI { get; set; }
            public int? LgTextSize { get; set; }
            public string LgCharacterSubstitutions { get; set; }
            public string LgRegexpSplitSentences { get; set; }
            public string LgExceptionsSplitSentences { get; set; }
            public string LgRegexpWordCharacters { get; set; }
            public bool? LgRemoveSpaces { get; set; }
            public bool? LgSplitEachChar { get; set; }
            public bool? LgRightToLeft { get; set; }
        }

        private class Sentences
        {
            public int? SeID { get; set; }
            public int? SeLgID { get; set; }
            public int? SeTxID { get; set; }
            public int? SeOrder { get; set; }
            public string SeText { get; set; }
        }

        private class Tags
        {
            public int? TgID { get; set; }
            public string TgText { get; set; }
            public string TgComment { get; set; }
        }

        private class Tags2
        {
            public int? T2ID { get; set; }
            public string T2Text { get; set; }
            public string T2Comment { get; set; }
        }

        private class TextItems
        {
            public int? TiID { get; set; }
            public int? TiLgID { get; set; }
            public int? TiTxID { get; set; }
            public int? TiSeID { get; set; }
            public int? TiOrder { get; set; }
            public int? TiWordCount { get; set; }
            public string TiText { get; set; }
            public string TiTextLC { get; set; }
            public bool? TiIsNotWord { get; set; }
        }

        private class Texts
        {
            public int? TxID { get; set; }
            public int? TxLgID { get; set; }
            public string TxTitle { get; set; }
            public string TxText { get; set; }
            public string TxAudioURI { get; set; }
        }

        private class TextTags
        {
            public int? TtTxID { get; set; }
            public int? TtT2ID { get; set; }
        }

        private class Words
        {
            public int? WoID { get; set; }
            public int? WoLgID { get; set; }
            public string WoText { get; set; }
            public string WoTextLC { get; set; }
            public int? WoStatus { get; set; }
            public string WoTranslation { get; set; }
            public string WoRomanization { get; set; }
            public string WoSentence { get; set; }
            public DateTime? WoCreated { get; set; }
            public DateTime? WoStatusChanged { get; set; }
            public double? WoTodayScore { get; set; }
            public double? WoTomorrowScore { get; set; }
            public double? WoRandom { get; set; }
        }

        private class WordTags
        {
            public int? WtWoID { get; set; }
            public int? WtTgID { get; set; }
        }
        #endregion
    }
}
