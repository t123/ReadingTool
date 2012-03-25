#region License
// WordsController.cs is part of ReadingTool.API
// 
// ReadingTool.API is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.API is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.API. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FluentMongo.Linq;
using MongoDB.Bson;
using ReadingTool.API.Areas.V1.Common;
using ReadingTool.API.Areas.V1.Models;
using ReadingTool.API.Common;
using ReadingTool.Common.Enums;
using ReadingTool.Common.Helpers;
using ReadingTool.Entities;
using ReadingTool.Services;

namespace ReadingTool.API.Areas.V1.Controllers
{
    public class WordsController : BaseController
    {
        private const int _maxWordsPerPage = 1000;

        public JsonNetResult Single(string wordId)
        {
            var data = _db.GetCollection<Word>(Collections.Words)
                .AsQueryable()
                .FirstOrDefault(x => x.Owner == _userId && x.WordId == new ObjectId(wordId));

            var mapped = Mapper.Map<Word, WordModel>(data);
            var reply = new WordResponse()
            {
                Words = new WordModel[] { mapped }
            };

            return new JsonNetResult() { Data = reply };
        }

        public JsonNetResult List()
        {
            if(Request.QueryString["count"] != null)
            {
                var count = _db.GetCollection<Word>(Collections.Words)
                .AsQueryable()
                .Count(x => x.Owner == _userId);

                return new JsonNetResult() { Data = new CountResponse() { Count = count, PerPage = _maxWordsPerPage } };
            }

            var data = _db.GetCollection<Word>(Collections.Words)
                .AsQueryable()
                .Where(x => x.Owner == _userId)
                .OrderBy(x => x.LanguageId)
                .ThenBy(x => x.WordPhraseLower)
                .Skip((_page - 1) * _maxWordsPerPage)
                .Take(_maxWordsPerPage)
                ;

            var mapped = Mapper.Map<IEnumerable<Word>, IEnumerable<WordModel>>(data);
            var reply = new WordResponse()
            {
                Words = mapped.ToArray()
            };

            return new JsonNetResult() { Data = reply };
        }

        public JsonNetResult ListByModified(string modified)
        {
            DateTime dt = DateTime.ParseExact(modified, "yyyyMMddHHmmss", null);

            if(Request.QueryString["count"] != null)
            {
                var count = _db.GetCollection<Word>(Collections.Words)
                .AsQueryable()
                .Count(x => x.Owner == _userId && x.Modified >= dt);

                return new JsonNetResult() { Data = new CountResponse() { Count = count, PerPage = _maxWordsPerPage } };
            }

            var data = _db.GetCollection<Word>(Collections.Words)
                .AsQueryable()
                .Where(x => x.Owner == _userId && x.Modified >= dt)
                .OrderBy(x => x.Modified)
                .Skip((_page - 1) * _maxWordsPerPage)
                .Take(_maxWordsPerPage)
                ;

            var mapped = Mapper.Map<IEnumerable<Word>, IEnumerable<WordModel>>(data);
            var reply = new WordResponse()
            {
                Words = mapped.ToArray()
            };

            return new JsonNetResult() { Data = reply };
        }

        public JsonNetResult ListByLanguage(string languageId)
        {
            if(Request.QueryString["count"] != null)
            {
                var count = _db.GetCollection<Word>(Collections.Words)
                .AsQueryable()
                .Count(x => x.Owner == _userId && x.LanguageId == new ObjectId(languageId));

                return new JsonNetResult() { Data = new CountResponse() { Count = count, PerPage = _maxWordsPerPage } };
            }

            var data = _db.GetCollection<Word>(Collections.Words)
                .AsQueryable()
                .Where(x => x.Owner == _userId && x.LanguageId == new ObjectId(languageId))
                .OrderBy(x => x.WordPhraseLower)
                .Skip((_page - 1) * _maxWordsPerPage)
                .Take(_maxWordsPerPage)
                ;

            var mapped = Mapper.Map<IEnumerable<Word>, IEnumerable<WordModel>>(data);
            var reply = new WordResponse()
            {
                Words = mapped.ToArray()
            };

            return new JsonNetResult() { Data = reply };
        }

        public JsonNetResult ListByLanguageModified(string languageId, string modified)
        {
            DateTime dt = DateTime.ParseExact(modified, "yyyyMMddHHmmss", null);

            if(Request.QueryString["count"] != null)
            {
                var count = _db.GetCollection<Word>(Collections.Words)
                .AsQueryable()
                .Count(x => x.Owner == _userId && x.Modified >= dt && x.LanguageId == new ObjectId(languageId));

                return new JsonNetResult() { Data = new CountResponse() { Count = count, PerPage = _maxWordsPerPage } };
            }

            var data = _db.GetCollection<Word>(Collections.Words)
                .AsQueryable()
                .Where(x => x.Owner == _userId && x.Modified >= dt && x.LanguageId == new ObjectId(languageId))
                .OrderBy(x => x.Modified)
                .Skip((_page - 1) * _maxWordsPerPage)
                .Take(_maxWordsPerPage)
                ;

            var mapped = Mapper.Map<IEnumerable<Word>, IEnumerable<WordModel>>(data);
            var reply = new WordResponse()
            {
                Words = mapped.ToArray()
            };

            return new JsonNetResult() { Data = reply };
        }

        public JsonNetResult Update(WordUpdateRequestModel request)
        {
            var language = _db.GetCollection<Language>(Collections.Languages)
                .AsQueryable()
                .FirstOrDefault(x => x.LanguageId == new ObjectId(request.LanguageId) && x.Owner == _userId);

            if(language == null)
            {
                var response = new ErrorResponse() { StatusCode = StatusCode.ClientError, StatusMessage = string.Format("{0} is not a valid language", request.LanguageId ?? "NOT SUPPLIED") };
                return new JsonNetResult() { Data = response };
            }

            var model = new WordUpdateResponseModel();
            var query = _db.GetCollection<Word>(Collections.Words).AsQueryable();
            IList<string> errorWords = new List<string>();

            foreach(var word in request.Words)
            {
                if(string.IsNullOrEmpty(word.WordPhrase))
                {
                    model.Skipped++;
                    continue;
                }

                try
                {
                    var found = query.FirstOrDefault(x => x.LanguageId == language.LanguageId && x.WordPhraseLower == word.WordPhrase.ToLowerInvariant().Trim());

                    if(found == null)
                    {
                        found = new Word()
                                    {
                                        BaseWord = (word.BaseWord ?? "").Trim(),
                                        WordPhrase = (word.WordPhrase ?? "").Trim(),
                                        Romanisation = (word.Romanisation ?? "").Trim(),
                                        Definition = (word.Definition ?? "").Trim(),
                                        Sentence = (word.Sentence ?? "").Trim(),
                                        LanguageName = language.Name,
                                        LanguageColour = language.Colour,
                                        LanguageId = language.LanguageId,
                                        SystemLanguageId = language.SystemLanguageId,
                                        Owner = _userId,
                                        Box = word.Box ?? 1,
                                        State = word.WordState ?? WordState.Unknown,
                                        Created = DateTime.Now,
                                        Modified = DateTime.Now,
                                        //ItemId = ObjectId.Empty,
                                        Tags = word.Tags,
                                        NextReview = DateTime.Now.AddMinutes(10),
                                    };

                        model.Additions++;
                    }
                    else
                    {
                        if(!string.IsNullOrEmpty(word.BaseWord))
                        {
                            found.BaseWord = word.BaseWord.Trim();
                        }

                        if(!string.IsNullOrEmpty(word.Romanisation))
                        {
                            found.Romanisation = word.Romanisation.Trim();
                        }

                        if(!string.IsNullOrEmpty(word.Definition))
                        {
                            found.Definition = word.Definition.Trim();
                        }

                        if(!string.IsNullOrEmpty(word.Sentence))
                        {
                            found.Sentence = word.Sentence.Trim();
                        }

                        if(word.Box.HasValue)
                        {
                            found.Box = word.Box.Value;
                        }

                        if(word.WordState.HasValue)
                        {
                            found.State = word.WordState.Value;
                        }

                        found.Tags = TagHelper.Merge(word.Tags);
                        found.Modified = DateTime.Now;

                        model.Updates++;
                    }

                    _db.GetCollection(Collections.Words).Save(found);
                }
                catch
                {
                    model.Errors++;
                }
            }

            model.StatusMessage = string.Join(";", errorWords);

            return new JsonNetResult() { Data = model };
        }
    }
}
