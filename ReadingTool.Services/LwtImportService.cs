using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using Newtonsoft.Json;
using ReadingTool.Core;
using ReadingTool.Core.Database;
using ReadingTool.Core.Enums;
using ReadingTool.Entities;

namespace ReadingTool.Services
{
    public interface ILwtImportService
    {
        void ImportJson(bool test, string mediaUrl, string data);
    }

    public class LwtImportService : ILwtImportService
    {
        private readonly MongoContext _context;
        private readonly ITextService _textService;
        private readonly ITermService _termService;
        private readonly ILanguageService _languageService;
        private readonly ISystemLanguageService _systemLanguageService;

        private readonly IUserIdentity _identity;

        public LwtImportService(
            MongoContext context,
            IPrincipal principal,
            ITextService textService,
            ITermService termService,
            ILanguageService languageService,
            ISystemLanguageService systemLanguageService
            )
        {
            _context = context;
            _textService = textService;
            _termService = termService;
            _languageService = languageService;
            _identity = principal.Identity as IUserIdentity;
            _systemLanguageService = systemLanguageService;
        }

        public void ImportJson(bool test, string mediaUrl, string data)
        {
            JsonLwt lwtData = new JsonLwt();
            lwtData = JsonConvert.DeserializeObject<JsonLwt>(data);
            string date = DateTime.Now.ToString("ddMM-HHmm");
            ObjectId owner = _identity.UserId;

            #region foreach language
            foreach(var item in lwtData.LWT_Languages)
            {
                try
                {
                    Language l = new Language()
                        {
                            Id = ObjectId.GenerateNewId(),
                            Name = item.LgName + "-" + date,
                            Owner = owner,
                            Modified = DateTime.Now,
                            SystemLanguageId = null,
                            Colour = "#FFFFFF",
                            IsPublic = false,
                            Review = null,
                            Settings = new LanguageSettings()
                                {
                                    Direction = item.LgRightToLeft.HasValue
                                                    ? item.LgRightToLeft.Value
                                                          ? LanguageDirection.RTL
                                                          : LanguageDirection.LTR
                                                    : LanguageDirection.LTR,
                                    ModalBehaviour = ModalBehaviour.LeftClick,
                                    ShowRomanisation = true,
                                    RemoveSpaces = item.LgRemoveSpaces ?? false,
                                    KeepFocus = true,
                                    CharacterSubstitutions = item.LgCharacterSubstitutions,
                                    RegexWordCharacters = item.LgRegexpWordCharacters,
                                    ExceptionSplitSentences = item.LgExceptionsSplitSentences,
                                    SplitEachCharacter = item.LgSplitEachChar ?? false,
                                    RegexSplitSentences = item.LgRegexpSplitSentences,
                                }
                        };

                    if(!string.IsNullOrEmpty(item.LgGoogleTranslateURI))
                    {
                        l.Dictionaries.Add(new LanguageDictionary()
                            {
                                AutoOpen = false,
                                DisplayOrder = 1,
                                Name = "Google Translate",
                                Parameter = DictionaryParameter.Sentence,
                                Url = item.LgGoogleTranslateURI,
                                Id = ObjectId.GenerateNewId(),
                                UrlEncoding = "",
                                WindowName = "googletranslation"
                            });
                    }

                    if(!string.IsNullOrEmpty(item.LgDict1URI))
                    {
                        l.Dictionaries.Add(new LanguageDictionary()
                        {
                            AutoOpen = false,
                            DisplayOrder = 2,
                            Name = "Dictionary 1",
                            Parameter = DictionaryParameter.Word,
                            Url = item.LgDict1URI,
                            Id = ObjectId.GenerateNewId(),
                            UrlEncoding = "",
                            WindowName = "dictionary_1"
                        });
                    }

                    if(!string.IsNullOrEmpty(item.LgDict2URI))
                    {
                        l.Dictionaries.Add(new LanguageDictionary()
                        {
                            AutoOpen = false,
                            DisplayOrder = 2,
                            Name = "Dictionary 2",
                            Parameter = DictionaryParameter.Word,
                            Url = item.LgDict2URI,
                            Id = ObjectId.GenerateNewId(),
                            UrlEncoding = "",
                            WindowName = "dictionary_2"
                        });
                    }

                    if(!test)
                    {
                        _languageService.Save(l);
                    }

                    item.Id = l.Id;
                }
                catch(Exception e)
                {
                    throw new Exception(string.Format("Could not import language: {0}", item.LgName), e);
                }
            }
            #endregion

            #region foreach archived text
            foreach(var item in lwtData.LWT_ArchivedTexts)
            {
                try
                {
                    var lid = lwtData.LWT_Languages.FirstOrDefault(x => x.LgID == item.AtLgID);
                    if(lid == null) throw new Exception("Could not find language with ID: " + item.AtLgID);

                    List<string> atags = new List<string>() { "archived" };

                    var text = new Text()
                        {
                            L1Id = lid.Id,
                            L2Id = null,
                            Title = item.AtTitle,
                            L1Text = item.AtText,
                            Owner = owner,
                            Modified = DateTime.Now,
                            L2Text = "",
                            IsParallel = false,
                        };

                    var links = lwtData.LWT_ArchTextTags.Where(x => x.AgAtID == item.AtId);
                    atags.AddRange(from i in links select lwtData.LWT_Tags2.FirstOrDefault(x => x.T2ID == i.AgT2ID) into t where t != null select t.T2Text);
                    text.Tags = TagHelper.Merge(atags.ToArray());
                    text.AudioUrl = (item.AtAudioURI ?? "").Contains("://") ? item.AtAudioURI : mediaUrl + item.AtAudioURI;

                    if(!test)
                    {
                        _textService.Save(text);
                    }
                }
                catch(Exception e)
                {
                    throw new Exception(string.Format("Could not import archived text: {0}", item.AtTitle), e);
                }
            }
            #endregion

            #region foreach text
            foreach(var item in lwtData.LWT_Texts)
            {
                try
                {
                    var lid = lwtData.LWT_Languages.FirstOrDefault(x => x.LgID == item.TxLgID);
                    if(lid == null) throw new Exception("Could not find language with ID: " + item.TxLgID);

                    List<string> atags = new List<string>();
                    var text = new Text()
                        {
                            L1Id = lid.Id,
                            L2Id = null,
                            Title = item.TxTitle,
                            L1Text = item.TxText,
                            Owner = owner,
                            Modified = DateTime.Now,
                            L2Text = "",
                            IsParallel = false,
                        };

                    text.AudioUrl = (item.TxAudioURI ?? "").Contains("://") ? item.TxAudioURI : mediaUrl + item.TxAudioURI;
                    var links = lwtData.LWT_TextTags.Where(x => x.TtTxID == item.TxID);
                    atags.AddRange(from i in links select lwtData.LWT_Tags2.FirstOrDefault(x => x.T2ID == i.TtT2ID) into t where t != null select t.T2Text);
                    text.Tags = TagHelper.Merge(atags.ToArray());

                    if(!test)
                    {
                        _textService.Save(text);
                    }
                }
                catch(Exception e)
                {
                    throw new Exception(string.Format("Could not import text: {0}", item.TxTitle), e);
                }
            }
            #endregion

            #region foreach word
            var terms = new List<Term>();
            foreach(var item in lwtData.LWT_Words)
            {
                try
                {
                    List<string> atags = new List<string>();
                    Term term = new Term()
                        {
                            Owner = owner,
                            LanguageId = lwtData.LWT_Languages.First(x => x.LgID == item.WoLgID).Id,
                            TermPhrase = (item.WoText ?? "").Replace("{", "").Replace("}", ""),
                            NextReview = DateTime.Now,
                        };

                    term.Length = (short)term.TermPhrase.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;

                    IndividualTerm it = new IndividualTerm
                        {
                            Created = item.WoCreated ?? DateTime.Now,
                            Modified = item.WoStatusChanged ?? DateTime.Now,
                            Sentence = (item.WoSentence ?? "").Replace("{", "").Replace("}", ""),
                            Definition = item.WoTranslation ?? "",
                            BaseTerm = "",
                            Romanisation = ""
                        };

                    if((item.WoStatus ?? 1) == 98)
                    {
                        term.Box = 1;
                        term.State = TermState.Ignored;
                    }
                    else if((item.WoStatus ?? 1) == 99)
                    {
                        term.State = TermState.Known;
                        term.Box = 9;
                    }
                    else
                    {
                        term.State = TermState.Unknown;
                        term.Box = item.WoStatus ?? 1;
                    }

                    var links = lwtData.LWT_WordTags.Where(x => x.WtWoID == item.WoID);
                    atags.AddRange(from i in links select lwtData.LWT_Tags.FirstOrDefault(x => x.TgID == i.WtTgID) into t where t != null select t.TgText);
                    it.Tags = TagHelper.Merge(atags.ToArray());
                    term.IndividualTerms.Add(it);

                    terms.Add(term);
                }
                catch(Exception e)
                {
                    throw new Exception(string.Format("Could not create term: {0}", item.WoText), e);
                }
            }

            if(!test)
            {
                try
                {
                    _termService.SaveAll(terms, audit: false);
                }
                catch(Exception e)
                {
                    throw new Exception(string.Format("Could not import terms"), e);
                }
            }
            #endregion
        }

        #region lwt classes
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
            public ObjectId Id { get; set; }


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
