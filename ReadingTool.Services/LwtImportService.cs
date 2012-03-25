#region License
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
// Copyright (C) 2012 Travis Watt
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using ReadingTool.Common.Enums;
using ReadingTool.Common.Helpers;
using ReadingTool.Entities;
using ReadingTool.Entities.Identity;
using ReadingTool.Entities.LWT;

namespace ReadingTool.Services
{
    public interface ILwtImportService
    {
        bool PingServer(Lwt model);
        void ImportData(Lwt model);
        void ImportDataJson(Lwt model);
    }

    public class LwtImportService : ILwtImportService
    {
        private readonly MongoDatabase _db;
        private readonly ISystemLanguageService _systemLanguageService;
        private readonly ITokeniserService _tokeniserService;
        private readonly UserForService _identity;
        private readonly ObjectId _userId;
        private JsonLwt _lwtData = new JsonLwt();

        public LwtImportService(
            MongoDatabase db,
            UserForService identity,
            ISystemLanguageService systemLanguageService,
            ITokeniserService tokeniserService
            )
        {
            _db = db;
            _systemLanguageService = systemLanguageService;
            _tokeniserService = tokeniserService;
            _identity = identity;
            _userId = identity.UserId;
        }

        public bool PingServer(Lwt model)
        {
            try
            {
                using(MySqlConnection conn = new MySqlConnection(model.ConnectionString))
                {
                    conn.Open();
                    conn.Close();
                }

                return true;
            }
            catch
            {
                return false;
            }
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

        private void GetSqlData(Lwt model)
        {
            using(MySqlConnection conn = new MySqlConnection(model.ConnectionString))
            {
                conn.Open();
                _lwtData.LWT_ArchivedTexts = GetArchivedTexts(conn);
                _lwtData.LWT_ArchTextTags = GetArchTextTags(conn);
                _lwtData.LWT_Languages = GetLanguages(conn);
                //var sentences = GetSentences(conn);
                _lwtData.LWT_Tags = GetTags(conn);
                _lwtData.LWT_Tags2 = GetTags2(conn);
                //var textitems = GetTextItems(conn);
                _lwtData.LWT_Texts = GetTexts(conn);
                _lwtData.LWT_TextTags = GetTextTags(conn);
                _lwtData.LWT_Words = GetWords(conn);
                _lwtData.LWT_WordTags = GetWordTags(conn);
                conn.Close();
            }
        }

        public void ImportData(Lwt model)
        {
            GetSqlData(model);
            DoImport(model);
        }

        public void ImportDataJson(Lwt model)
        {
            _lwtData = JsonConvert.DeserializeObject<JsonLwt>(model.JsonData);
            DoImport(model);
        }

        private void DoImport(Lwt model)
        {
            string date = DateTime.Now.ToString("ddMM-HHmm");
            ObjectId owner = _userId;
            var systemLanguage = _systemLanguageService.FindByCode(SystemLanguage.NotYetSetCode);

            Regex regex = new Regex(model.NumberCollectionRegEx);

            foreach(var item in _lwtData.LWT_Languages)
            {
                Language l = new Language()
                {
                    LanguageId = ObjectId.GenerateNewId(),
                    Name = item.LgName + "-" + date,
                    IsRtlLanguage = item.LgRightToLeft ?? false,
                    TranslateUrl = item.LgGoogleTranslateURI ?? "",
                    Owner = owner,
                    Created = DateTime.Now,
                    Modified = DateTime.Now,
                    ModalBehaviour = ModalBehaviour.LeftClick,
                    HasRomanisationField = true,
                    SystemLanguageId = systemLanguage.SystemLanguageId,
                    RemoveSpaces = item.LgRemoveSpaces ?? false,
                    Punctuation = @"» « , ! : \ / & £ % ~ @ ; # ] } "" ^ $ ( ) < [ { \ | > . * + ? “ ” 。 ― 、… -".Split(' '),
                    SentenceEndRegEx = @"[.?!。]|$",
                    KeepFocus = true
                };

                if(!string.IsNullOrEmpty(item.LgDict1URI)) l.Dictionaries.Add(new UserDictionary() { Name = "Dictionary 1", Url = item.LgDict1URI, WindowName = "dict1" });
                if(!string.IsNullOrEmpty(item.LgDict2URI)) l.Dictionaries.Add(new UserDictionary() { Name = "Dictionary 2", Url = item.LgDict2URI, WindowName = "dict2" });
                //if(l.Dictionaries.Count > 0) l.DefaultDictionary = "Dictionary 1"; //Evilness!

                if(!model.TestMode)
                {
                    _db.GetCollection(Collections.Languages).Save(l);
                }

                item.Id = l.LanguageId;
            }

            foreach(var item in _lwtData.LWT_ArchivedTexts)
            {
                var lid = _lwtData.LWT_Languages.First(x => x.LgID == item.AtLgID);
                if(lid == null) throw new Exception("Could not find language with ID: " + item.AtLgID);

                List<string> atags = new List<string>() { "archived" };

                var text = new Item()
                {
                    LanguageId = lid.Id,
                    Title = item.AtTitle,
                    L1Text = item.AtText,
                    Owner = owner,
                    Created = DateTime.Now,
                    Modified = DateTime.Now,
                    L2Text = "",
                    LanguageName = lid.LgName,
                    LanguageColour = "",
                    ItemType = ItemType.Text,
                    SystemLanguageId = systemLanguage.SystemLanguageId
                };

                var links = _lwtData.LWT_ArchTextTags.Where(x => x.AgAtID == item.AtId);
                atags.AddRange(from i in links select _lwtData.LWT_Tags2.FirstOrDefault(x => x.T2ID == i.AgT2ID) into t where t != null select t.T2Text);
                text.Tags = TagHelper.Merge(atags.ToArray());
                text.Url = (item.AtAudioURI ?? "").Contains("://") ? item.AtAudioURI : model.MediaUrl + item.AtAudioURI;

                if(!string.IsNullOrEmpty(model.NumberCollectionRegEx))
                {
                    var m = regex.Match(text.Title);

                    if(m.Success)
                    {
                        var value = m.Value.Replace("-", "").Replace(":", "").Trim();
                        int no;
                        if(int.TryParse(value, out no)) text.CollectionNo = no;
                    }
                }

                if(model.TagsToCollections)
                {
                    var collectionNames =
                        from i in links
                        select _lwtData.LWT_Tags2.FirstOrDefault(x => x.T2ID == i.AgT2ID)
                            into t
                            where t != null
                            select t.T2Text;

                    var collectionName = collectionNames.FirstOrDefault(x => !string.IsNullOrEmpty(x));
                    if(!string.IsNullOrEmpty(collectionName)) text.CollectionName = collectionName;
                }
                else
                {
                    text.CollectionName = "";
                }

                if(!model.TestMode)
                {
                    _db.GetCollection(Collections.Items).Save(text);
                }
            }

            foreach(var item in _lwtData.LWT_Texts)
            {
                List<string> atags = new List<string>();
                var text = new Item()
                {
                    Title = item.TxTitle,
                    L1Text = item.TxText,
                    LanguageId = _lwtData.LWT_Languages.First(x => x.LgID == item.TxLgID).Id,
                    Owner = owner,
                    Created = DateTime.Now,
                    Modified = DateTime.Now,
                    L2Text = "",
                    LanguageName = _lwtData.LWT_Languages.First(x => x.LgID == item.TxLgID).LgName,
                    LanguageColour = "",
                    ItemType = ItemType.Text,
                    SystemLanguageId = systemLanguage.SystemLanguageId
                };

                text.Url = (item.TxAudioURI ?? "").Contains("://") ? item.TxAudioURI : model.MediaUrl + item.TxAudioURI;
                var links = _lwtData.LWT_TextTags.Where(x => x.TtTxID == item.TxID);
                atags.AddRange(from i in links select _lwtData.LWT_Tags2.FirstOrDefault(x => x.T2ID == i.TtT2ID) into t where t != null select t.T2Text);
                text.Tags = TagHelper.Merge(atags.ToArray());

                if(!string.IsNullOrEmpty(model.NumberCollectionRegEx))
                {
                    var m = regex.Match(text.Title);

                    if(m.Success)
                    {
                        var value = m.Value.Replace("-", "").Replace(":", "").Trim();
                        int no;
                        if(int.TryParse(value, out no)) text.CollectionNo = no;
                    }
                }

                if(model.TagsToCollections)
                {
                    var collectionNames =
                        from i in links
                        select _lwtData.LWT_Tags2.FirstOrDefault(x => x.T2ID == i.TtT2ID)
                            into t
                            where t != null
                            select t.T2Text;

                    var collectionName = collectionNames.FirstOrDefault(x => !string.IsNullOrEmpty(x));
                    if(!string.IsNullOrEmpty(collectionName)) text.CollectionName = collectionName;
                }
                else
                {
                    text.CollectionName = "";
                }

                if(!model.TestMode)
                {
                    _db.GetCollection(Collections.Items).Save(text);
                }
            }

            foreach(var item in _lwtData.LWT_Words)
            {
                List<string> atags = new List<string>();
                Word word = new Word()
                                {
                                    Created = item.WoCreated ?? DateTime.Now,
                                    Modified = item.WoStatusChanged ?? DateTime.Now,
                                    Owner = owner,
                                    LanguageId = _lwtData.LWT_Languages.First(x => x.LgID == item.WoLgID).Id,
                                    LanguageName = _lwtData.LWT_Languages.First(x => x.LgID == item.WoLgID).LgName,
                                    LanguageColour = "",
                                    Definition = item.WoTranslation ?? "",
                                    Sentence = (item.WoSentence ?? "").Replace("{", "").Replace("}", ""),
                                    WordPhrase = (item.WoText ?? "").Replace("{", "").Replace("}", ""),
                                    SystemLanguageId = ObjectId.Empty
                                };

                if(model.UseRomanisationAsBaseWord)
                {
                    word.BaseWord = item.WoRomanization;
                    word.Romanisation = string.Empty;
                }
                else
                {
                    word.BaseWord = string.Empty;
                    word.Romanisation = item.WoRomanization;
                }

                if((item.WoStatus ?? 1) == 98)
                {
                    word.Box = 1;
                    word.State = WordState.Ignored;
                }
                else if((item.WoStatus ?? 1) == 99)
                {
                    word.State = WordState.Known;
                    word.Box = 9;
                }
                else
                {
                    word.State = WordState.Unknown;
                    word.Box = item.WoStatus ?? 1;
                }

                var links = _lwtData.LWT_WordTags.Where(x => x.WtWoID == item.WoID);
                atags.AddRange(from i in links select _lwtData.LWT_Tags.FirstOrDefault(x => x.TgID == i.WtTgID) into t where t != null select t.TgText);
                word.Tags = TagHelper.Merge(atags.ToArray());

                if(!model.TestMode)
                {
                    _db.GetCollection(Collections.Words).Save(word);
                }
            }
        }

        #region convert data fields
        private string GetString(object incoming)
        {
            if(incoming == null) return "";
            return incoming.ToString();
        }

        private int? GetInt(object incoming)
        {
            if(incoming == null) return null;
            int parse;
            if(!int.TryParse(incoming.ToString(), out parse)) return null;
            return parse;
        }

        private bool? GetBool(object incoming)
        {
            if(incoming == null) return null;
            bool parse;
            if(!bool.TryParse(incoming.ToString(), out parse)) return null;
            return parse;
        }

        private double? GetDouble(object incoming)
        {
            if(incoming == null) return null;
            double parse;
            if(!double.TryParse(incoming.ToString(), out parse)) return null;
            return parse;
        }

        private DateTime? GetDateTime(object incoming)
        {
            if(incoming == null) return null;
            DateTime parse;
            if(!DateTime.TryParse(incoming.ToString(), out parse)) return null;
            return parse;
        }
        #endregion

        private MySqlDataReader Read(MySqlConnection conn, string command)
        {
            using(MySqlCommand cmd = new MySqlCommand(command, conn))
            {
                return cmd.ExecuteReader(CommandBehavior.Default);
            }
        }

        #region read in all the data
        private IList<ArchivedTexts> GetArchivedTexts(MySqlConnection conn)
        {
            IList<ArchivedTexts> archivedtexts = new List<ArchivedTexts>();

            using(MySqlDataReader reader = Read(conn, "SELECT * FROM archivedtexts"))
            {
                while(reader.Read())
                {
                    archivedtexts.Add(new ArchivedTexts()
                    {
                        AtId = GetInt(reader["AtID"]),
                        AtLgID = GetInt(reader["AtLgID"]),
                        AtTitle = GetString(reader["AtTitle"]),
                        AtText = GetString(reader["AtText"]),
                        AtAudioURI = GetString(reader["AtAudioURI"])
                    });
                }
            }

            return archivedtexts;
        }

        private IList<ArchTextTags> GetArchTextTags(MySqlConnection conn)
        {
            IList<ArchTextTags> archtexttags = new List<ArchTextTags>();

            using(var reader = Read(conn, "SELECT * FROM archtexttags"))
            {
                while(reader.Read())
                {
                    archtexttags.Add(new ArchTextTags()
                    {
                        AgAtID = GetInt(reader["AgAtID"]),
                        AgT2ID = GetInt(reader["AgT2ID"]),
                    });
                }
            }

            return archtexttags;
        }

        private IList<Languages> GetLanguages(MySqlConnection conn)
        {
            IList<Languages> languages = new List<Languages>();
            using(var reader = Read(conn, "SELECT * FROM languages"))
            {
                while(reader.Read())
                {
                    languages.Add(new Languages()
                    {
                        LgID = GetInt(reader["LgID"]),
                        LgName = GetString(reader["LgName"]),
                        LgDict1URI = GetString(reader["LgDict1URI"]),
                        LgDict2URI = GetString(reader["LgDict2URI"]),
                        LgGoogleTranslateURI = GetString(reader["LgGoogleTranslateURI"]),
                        LgGoogleTTSURI = GetString(reader["LgGoogleTTSURI"]),
                        LgTextSize = GetInt(reader["LgTextSize"]),
                        LgCharacterSubstitutions = GetString(reader["LgCharacterSubstitutions"]),
                        LgRegexpSplitSentences = GetString(reader["LgRegexpSplitSentences"]),
                        LgExceptionsSplitSentences = GetString(reader["LgExceptionsSplitSentences"]),
                        LgRegexpWordCharacters = GetString(reader["LgRegexpWordCharacters"]),
                        LgRemoveSpaces = GetBool(reader["LgRemoveSpaces"]),
                        LgSplitEachChar = GetBool(reader["LgSplitEachChar"]),
                        LgRightToLeft = GetBool(reader["LgRightToLeft"])
                    });
                }
            }

            return languages;
        }

        private IList<Sentences> GetSentences(MySqlConnection conn)
        {
            IList<Sentences> sentences = new List<Sentences>();

            using(var reader = Read(conn, "SELECT * FROM sentences"))
            {
                while(reader.Read())
                {
                    sentences.Add(new Sentences()
                    {
                        SeID = GetInt(reader["SeID"]),
                        SeLgID = GetInt(reader["SeLgID"]),
                        SeTxID = GetInt(reader["SeTxID"]),
                        SeOrder = GetInt(reader["SeOrder"]),
                        SeText = GetString(reader["SeText"])
                    });
                }
            }

            return sentences;
        }

        private IList<Tags> GetTags(MySqlConnection conn)
        {
            var result = new List<Tags>();

            using(var reader = Read(conn, "SELECT * FROM tags"))
            {
                while(reader.Read())
                {
                    result.Add(new Tags()
                    {
                        TgID = GetInt(reader["TgID"]),
                        TgText = GetString(reader["TgText"]),
                        TgComment = GetString(reader["TgComment"])
                    });
                }
            }

            return result;
        }

        private IList<Tags2> GetTags2(MySqlConnection conn)
        {
            var result = new List<Tags2>();

            using(var reader = Read(conn, "SELECT * FROM tags2"))
            {
                while(reader.Read())
                {
                    result.Add(new Tags2()
                    {
                        T2ID = GetInt(reader["T2ID"]),
                        T2Text = GetString(reader["T2Text"]),
                        T2Comment = GetString(reader["T2Comment"])
                    });
                }
            }

            return result;
        }

        private IList<TextItems> GetTextItems(MySqlConnection conn)
        {
            var result = new List<TextItems>();

            using(var reader = Read(conn, "SELECT * FROM textitems"))
            {
                while(reader.Read())
                {
                    result.Add(new TextItems()
                    {
                        TiID = GetInt(reader["TiID"]),
                        TiLgID = GetInt(reader["TiLgID"]),
                        TiTxID = GetInt(reader["TiTxID"]),
                        TiSeID = GetInt(reader["TiSeID"]),
                        TiOrder = GetInt(reader["TiOrder"]),
                        TiWordCount = GetInt(reader["TiWordCount"]),
                        TiText = GetString(reader["TiText"]),
                        TiTextLC = GetString(reader["TiTextLC"]),
                        TiIsNotWord = GetBool(reader["TiIsNotWord"])
                    });
                }
            }

            return result;
        }

        private IList<Texts> GetTexts(MySqlConnection conn)
        {
            var result = new List<Texts>();

            using(var reader = Read(conn, "SELECT * FROM texts"))
            {
                while(reader.Read())
                {
                    result.Add(new Texts()
                    {
                        TxID = GetInt(reader["TxID"]),
                        TxLgID = GetInt(reader["TxLgID"]),
                        TxTitle = GetString(reader["TxTitle"]),
                        TxText = GetString(reader["TxText"]),
                        TxAudioURI = GetString(reader["TxAudioURI"])
                    });
                }
            }

            return result;
        }

        private IList<TextTags> GetTextTags(MySqlConnection conn)
        {
            var result = new List<TextTags>();

            using(var reader = Read(conn, "SELECT * FROM texttags"))
            {
                while(reader.Read())
                {
                    result.Add(new TextTags()
                    {
                        TtTxID = GetInt(reader["TtTxID"]),
                        TtT2ID = GetInt(reader["TtT2ID"])
                    });
                }
            }

            return result;
        }

        private IList<Words> GetWords(MySqlConnection conn)
        {
            var result = new List<Words>();

            using(var reader = Read(conn, "SELECT * FROM words"))
            {
                while(reader.Read())
                {
                    result.Add(new Words()
                    {
                        WoID = GetInt(reader["WoID"]),
                        WoLgID = GetInt(reader["WoLgID"]),
                        WoText = GetString(reader["WoText"]),
                        WoTextLC = GetString(reader["WoTextLC"]),
                        WoStatus = GetInt(reader["WoStatus"]),
                        WoTranslation = GetString(reader["WoTranslation"]),
                        WoRomanization = GetString(reader["WoRomanization"]),
                        WoSentence = GetString(reader["WoSentence"]),
                        WoCreated = GetDateTime(reader["WoCreated"]),
                        WoStatusChanged = GetDateTime(reader["WoStatusChanged"]),
                        WoTodayScore = GetDouble(reader["WoTodayScore"]),
                        WoTomorrowScore = GetDouble(reader["WoTomorrowScore"]),
                        WoRandom = GetDouble(reader["WoRandom"]),
                    });
                }
            }

            return result;
        }

        private IList<WordTags> GetWordTags(MySqlConnection conn)
        {
            var result = new List<WordTags>();

            using(var reader = Read(conn, "SELECT * FROM wordtags"))
            {
                while(reader.Read())
                {
                    result.Add(new WordTags()
                    {
                        WtWoID = GetInt(reader["WtWoID"]),
                        WtTgID = GetInt(reader["WtTgID"])
                    });
                }
            }

            return result;
        }
        #endregion
    }
}
