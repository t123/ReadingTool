#region License
// WordService.cs is part of ReadingTool.Services
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
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using FluentMongo.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using ReadingTool.Common;
using ReadingTool.Common.Enums;
using ReadingTool.Common.Helpers;
using ReadingTool.Common.Keys;
using ReadingTool.Entities;
using ReadingTool.Entities.Identity;
using ReadingTool.Entities.Search;

namespace ReadingTool.Services
{
    public interface IWordService
    {
        Word FindWordByLower(string languageId, string word);

        Word SaveWord(
            string languageId, string word, string state, string baseWord, string romanisation,
            string defintiion, string tags, string sentence, string itemId);

        Word SaveWord(string languageId, string word, string state, string itemId, string sentence);
        bool MarkRemainingAsKnown(string languageId, string[] words, string itemId);
        //IEnumerable<Word> FindAllForParsing(ObjectId languageId);
        Tuple<ParsingWord[], ParsingWord[]> FindAllForParsingSplit(ObjectId languageId);
        IEnumerable<string> AutocompleteTags(string term);
        IEnumerable<string> AutocompleteTags(string term, int limit);
        void Delete(string id);
        void Delete(ObjectId id);
        void SaveReviews(Dictionary<ObjectId, string> result);
        Word FindOne(string id);
        Word FindOne(ObjectId id);
        void Save(Word word);
        IEnumerable<Word> FindAllForOwner();
        IEnumerable<Word> FindAllForReview(ObjectId languageId, int numberOfWords);
        IEnumerable<Word> FindAllById(string[] id);
        IEnumerable<Word> FindAllById(ObjectId[] id);
        IEnumerable<Word> FindAllForLanguage(ObjectId id);
        IEnumerable<Word> SearchWords(string filter, string[] boxes, string[] languages, string[] states, string[] tags);

        int FindKnownCountForUser(ObjectId userId, ObjectId languageId);
        //MongoCursor<Word> SearchWords(string filter, string[] languages, string[] states, string[] boxes);
        SearchResult<Word> SearchWords(string filter, string[] languages, string[] states, string[] boxes, string orderBy, string orderDirection, int limit, int page);
        IEnumerable<Word> FindSharedDefinitions(User user, string word, ObjectId systemLanguage);
    }

    public class WordService : IWordService
    {
        private readonly MongoDatabase _db;
        private readonly UserForService _identity;

        public WordService(
            MongoDatabase db,
            UserForService identity
            )
        {
            _db = db;
            _identity = identity;
        }

        public Word FindWordByLower(string languageId, string word)
        {
            string wordLower = (word ?? "").ToLowerInvariant();
            return _db.GetCollection<Word>(Collections.Words)
                .AsQueryable()
                .FirstOrDefault(x =>
                    x.WordPhraseLower == wordLower &&
                    x.LanguageId == new ObjectId(languageId) &&
                    x.Owner == _identity.UserId
                    );
        }

        private Language FindLanguage(string languageId)
        {
            ObjectId id;

            if(!ObjectId.TryParse(languageId, out id))
                return null;

            //TOOD evilness, shouldn't be here
            return _db.GetCollection<Language>(Collections.Languages)
                .AsQueryable()
                .FirstOrDefault(x => x.LanguageId == id && x.Owner == _identity.UserId);
        }

        public Word SaveWord(
            string languageId, string word, string state, string baseWord, string romanisation,
            string defintiion, string tags, string sentence, string itemId)
        {
            word = word.Trim();
            var newWord = FindWordByLower(languageId, word);

            if(newWord == null)
            {
                Language l = FindLanguage(languageId) ?? Language.Unknown();

                newWord = new Word()
                              {
                                  Created = DateTime.Now,
                                  Modified = DateTime.Now,
                                  LanguageId = new ObjectId(languageId),
                                  Owner = _identity.UserId,
                                  State = GetEnumFromAlternateName(state),
                                  //ItemId = string.IsNullOrEmpty(itemId) ? ObjectId.Empty : new ObjectId(itemId),
                                  WordPhrase = (word ?? "").Trim(),
                                  BaseWord = (baseWord ?? "").Trim(),
                                  Box = 1,
                                  NextReview = DateTime.Now.AddMinutes(10),
                                  Definition = (defintiion ?? "").Trim(),
                                  Romanisation = (romanisation ?? "").Trim(),
                                  Tags = TagHelper.Split(tags),
                                  Sentence = (sentence ?? "").Trim(),
                                  LanguageColour = l.Colour,
                                  LanguageName = l.Name,
                                  SystemLanguageId = l.SystemLanguageId
                              };
            }
            else
            {
                newWord.Modified = DateTime.Now;
                newWord.State = GetEnumFromAlternateName(state);
                newWord.BaseWord = (baseWord ?? "").Trim();
                newWord.Romanisation = (romanisation ?? "").Trim();
                newWord.Definition = (defintiion ?? "").Trim();
                newWord.Sentence = string.IsNullOrEmpty(sentence) ? (newWord.Sentence ?? "").Trim() : (sentence ?? "").Trim();
                newWord.Tags = TagHelper.Split(tags);
            }

            _db.GetCollection(Collections.Words).Save(newWord);

            return newWord;
        }

        private static WordState GetEnumFromAlternateName(string name)
        {
            FieldInfo[] fi = typeof(WordState).GetFields();
            foreach(var field in fi)
            {
                AlternateNameAttribute[] attributes = (AlternateNameAttribute[])field.GetCustomAttributes(typeof(AlternateNameAttribute), false);

                if(attributes.Length > 0 && attributes[0].Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return (WordState)field.GetValue(null);
                }
            }

            throw new Exception("Alternate name not found");
        }

        public Word SaveWord(string languageId, string word, string state, string itemId, string sentence)
        {
            var newState = GetEnumFromAlternateName(state);
            var newWord = FindWordByLower(languageId, word);

            if(newWord == null)
            {
                Language l = FindLanguage(languageId) ?? Language.Unknown();

                newWord = new Word()
                {
                    Created = DateTime.Now,
                    Modified = DateTime.Now,
                    LanguageId = new ObjectId(languageId),
                    Owner = _identity.UserId,
                    State = newState,
                    //ItemId = string.IsNullOrEmpty(itemId) ? ObjectId.Empty : new ObjectId(itemId),
                    WordPhrase = word,
                    BaseWord = string.Empty,
                    Box = 1,
                    NextReview = DateTime.Now.AddMinutes(10),
                    Definition = string.Empty,
                    Romanisation = string.Empty,
                    Tags = new string[] { },
                    Sentence = sentence,
                    LanguageColour = l.Colour,
                    LanguageName = l.Name,
                    SystemLanguageId = l.SystemLanguageId
                };
            }
            else
            {
                newWord.Modified = DateTime.Now;
                newWord.State = newState;
                if(string.IsNullOrEmpty(newWord.Sentence)) newWord.Sentence = sentence;
            }

            _db.GetCollection(Collections.Words).Save(newWord);

            return newWord;
        }

        public bool MarkRemainingAsKnown(string languageId, string[] words, string itemId)
        {
            if(words == null || words.Length == 0) return true;

            MongoInsertOptions options = new MongoInsertOptions(_db.GetCollection(Collections.Words));
            options.SafeMode = SafeMode.Create(true);
            IEnumerable<SafeModeResult> result;

            IList<Word> wordList = new List<Word>();
            Language l = FindLanguage(languageId) ?? Language.Unknown();

            foreach(var word in words.Distinct(StringComparer.Create(CultureInfo.InvariantCulture, true)))
            {
                if(word.Trim().Equals("")) continue;

                wordList.Add(
                    new Word()
                        {
                            Created = DateTime.Now,
                            Modified = DateTime.Now,
                            LanguageId = new ObjectId(languageId),
                            Owner = _identity.UserId,
                            State = WordState.Known,
                            //ItemId = string.IsNullOrEmpty(itemId) ? ObjectId.Empty : new ObjectId(itemId),
                            BaseWord = string.Empty,
                            Box = 1,
                            NextReview = DateTime.Now.AddMinutes(10),
                            Definition = string.Empty,
                            Romanisation = string.Empty,
                            Tags = new string[] { },
                            Sentence = string.Empty,
                            WordPhrase = word,
                            LanguageColour = l.Colour,
                            LanguageName = l.Name
                        }
                    );
            }

            try
            {
                result = _db.GetCollection(Collections.Words).InsertBatch(wordList, options);
            }
            catch(MongoSafeModeException exception)
            {
                return false;
            }
            catch
            {
                return false;
            }

            return true;
        }

        //public IEnumerable<Word> FindAllForParsing(ObjectId languageId)
        //{
        //    return _db.GetCollection<Word>(Collections.Words)
        //        .AsQueryable()
        //        .Where(x => x.LanguageId == languageId && x.Owner == _identity.UserId);
        //}

        public Tuple<ParsingWord[], ParsingWord[]> FindAllForParsingSplit(ObjectId languageId)
        {
            var collection = _db.GetCollection<Word>(Collections.Words).AsQueryable();
            return new Tuple<ParsingWord[], ParsingWord[]>
                (
                collection.Where(x => x.LanguageId == languageId && x.Owner == _identity.UserId && x.Length == 1)
                    .Select(x => new ParsingWord()
                                     {
                                         Length = x.Length,
                                         State = x.State,
                                         WordPhrase = x.WordPhrase,
                                         WordPhraseLower = x.WordPhraseLower,
                                         WordId = x.WordId,
                                         BaseWord = x.BaseWord,
                                         Definition = x.Definition,
                                         Romanisation = x.Romanisation
                                     })
                    .ToArray(),
                collection.Where(x => x.LanguageId == languageId && x.Owner == _identity.UserId && x.Length > 1)
                    .Select(x => new ParsingWord()
                                     {
                                         Length = x.Length,
                                         State = x.State,
                                         WordPhrase = x.WordPhrase,
                                         WordPhraseLower = x.WordPhraseLower,
                                         WordId = x.WordId,
                                         BaseWord = x.BaseWord,
                                         Definition = x.Definition,
                                         Romanisation = x.Romanisation
                                     })
                    .ToArray()
                );
        }

        public IEnumerable<string> AutocompleteTags(string term)
        {
            return AutocompleteTags(term, 10);
        }

        public IEnumerable<string> AutocompleteTags(string term, int limit)
        {
            //TODO fixme to filter in DB
            var results =
                _db.GetCollection<Word>(Collections.Words)
                .Distinct("Tags", Query.EQ("Owner", _identity.UserId))
                .Select(x => x.AsString)
                .Where(x => x.StartsWith(term, StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(x => x);

            return limit < 0 ? results : results.Take(limit);
        }

        public void Delete(string id)
        {
            Delete(new ObjectId(id));
        }

        public void Delete(ObjectId id)
        {
            _db.GetCollection<Word>(Collections.Words).Remove(
                Query.And(
                    Query.EQ("Owner", _identity.UserId),
                    Query.EQ("_id", id)
                    )
                );
        }

        public void SaveReviews(Dictionary<ObjectId, string> result)
        {
            foreach(var update in result)
            {
                Word word = FindOne(update.Key);
                if(word == null) continue;

                switch(update.Value)
                {
                    case "I": continue;
                    case "K":
                        word.State = WordState.Known;
                        break;

                    case "R":
                        Random r = HttpRuntime.Cache[CacheKeys.RANDOM] as Random;
                        var hours = Math.Pow(3, word.Box) * 24 + (r.Next(-12, 12));
                        word.NextReview = DateTime.Now.AddHours(hours);
                        word.Box++;
                        break;

                    case "F":
                        word.Box = 1;
                        word.NextReview = DateTime.Now.AddMinutes(10);
                        break;

                    default:
                        continue;
                }

                Save(word);
            }
        }

        public Word FindOne(string id)
        {
            return FindOne(new ObjectId(id));
        }

        public Word FindOne(ObjectId id)
        {
            return _db.GetCollection<Word>(Collections.Words)
                .AsQueryable()
                .FirstOrDefault(x => x.WordId == id && x.Owner == _identity.UserId);
        }

        /// <summary>
        /// NB: There is a different BatchSave in MarkRemainingKnown
        /// </summary>
        /// <param name="word"></param>
        public void Save(Word word)
        {
            if(word == null) return;

            if(word.WordId == ObjectId.Empty)
            {
                word.Created = DateTime.Now;
                word.Owner = _identity.UserId;
                word.Box = 1;
                word.NextReview = DateTime.Now.AddMinutes(10);
            }

            word.Romanisation = (word.Romanisation ?? "").Trim();
            word.Definition = (word.Definition ?? "").Trim();
            word.BaseWord = (word.BaseWord ?? "").Trim();
            word.Sentence = (word.Sentence ?? "").Trim();

            word.Modified = DateTime.Now;
            _db.GetCollection(Collections.Words).Save(word);
        }

        public IEnumerable<Word> FindAllForOwner()
        {
            return _db.GetCollection<Word>(Collections.Words)
                .AsQueryable()
                .Where(x => x.Owner == _identity.UserId)
                .OrderBy(x => x.WordPhraseLower)
                ;
        }

        public IEnumerable<Word> FindAllForReview(ObjectId languageId, int numberOfWords)
        {
            return _db.GetCollection<Word>(Collections.Words)
                .AsQueryable()
                .Where(x =>
                       x.Owner == _identity.UserId &&
                       x.NextReview <= DateTime.Now &&
                       x.LanguageId == languageId &&
                       x.State == WordState.Unknown
                )
                .Take(numberOfWords);
        }

        public IEnumerable<Word> FindAllById(string[] id)
        {
            return FindAllById(id.Select(x => new ObjectId(x)).ToArray());
        }

        public IEnumerable<Word> FindAllById(ObjectId[] id)
        {
            return _db.GetCollection<Word>(Collections.Words)
                .AsQueryable()
                .Where(x => x.Owner == _identity.UserId && id.Contains(x.WordId));
        }

        public IEnumerable<Word> FindAllForLanguage(ObjectId id)
        {
            return _db.GetCollection<Word>(Collections.Words)
                .AsQueryable()
                .Where(x => x.Owner == _identity.UserId && x.LanguageId == id);
        }

        public IEnumerable<Word> SearchWords(string filter, string[] boxes, string[] languages, string[] states, string[] tags)
        {
            var words = _db.GetCollection<Word>(Collections.Words);

            var queries = new List<IMongoQuery>() { Query.EQ("Owner", _identity.UserId) };

            if(!(languages.Length == 1 && languages[0].Equals("")))
                queries.Add(Query.In("LanguageId", new BsonArray(languages.Select(x => new ObjectId(x)))));

            if(!(states.Length == 1 && states[0].Equals("")))
            {
                queries.Add(Query.In("State", new BsonArray(states.Select(x => Enum.Parse(typeof(WordState), x, true)))));
            }

            if(!(boxes.Length == 1 && boxes[0].Equals("")))
            {
                queries.Add(Query.In("Box", new BsonArray(boxes.Select(int.Parse))));
            }

            if(!(tags.Length == 1 && tags[0].Equals("")))
                queries.Add(Query.In("Tags", new BsonArray(tags)));

            if(!string.IsNullOrEmpty(filter))
            {
                queries.Add(Query.Matches("WordPhraseLower", BsonRegularExpression.Create(new Regex(filter.ToLowerInvariant()))));
            }

            var cursor = words.Find(Query.And(queries.ToArray()));

            return cursor;
        }

        public int FindKnownCountForUser(ObjectId userId, ObjectId languageId)
        {
            return _db.GetCollection<Word>(Collections.Words)
                .AsQueryable()
                .Count(x => x.Owner == userId && x.LanguageId == languageId && x.State == WordState.Known);
        }

        public SearchResult<Word> SearchWords(string filter, string[] languages, string[] states, string[] boxes, string orderBy, string orderDirection, int limit, int page)
        {
            filter = (filter ?? "").ToLowerInvariant().Trim();
            var words = _db.GetCollection<Word>(Collections.Words);

            var queries = new List<IMongoQuery>() { Query.EQ("Owner", _identity.UserId) };

            if(!(languages.Length == 1 && languages[0].Equals("")))
                queries.Add(Query.In("LanguageId", new BsonArray(languages.Select(x => new ObjectId(x)))));

            string[] splitFilter = filter.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string[] tags = splitFilter.Where(x => x.StartsWith("#")).Select(x => x.Substring(1, x.Length - 1)).ToArray();
            filter = string.Join(" ", splitFilter.Where(x => !x.StartsWith("#")));

            if(!(states.Length == 1 && states[0].Equals("")))
            {
                queries.Add(Query.In("State", new BsonArray(states.Select(x => Enum.Parse(typeof(WordState), x, true)))));
            }

            if(!(boxes.Length == 1 && boxes[0].Equals("")))
            {
                queries.Add(Query.In("Box", new BsonArray(boxes.Select(int.Parse))));
            }

            if(tags.Length > 0)
                queries.Add(Query.In("Tags", new BsonArray(tags)));

            if(!string.IsNullOrEmpty(filter))
            {
                queries.Add(Query.Matches("WordPhraseLower", BsonRegularExpression.Create(new Regex(filter))));
            }

            var cursor = words.Find(Query.And(queries.ToArray()));

            switch(orderBy)
            {
                case "state":
                    cursor.SetSortOrder(orderDirection == "asc"
                        ? SortBy.Ascending("State", "LanguageName", "WordPhraseLower")
                        : SortBy.Descending("State").Ascending("LanguageName", "WordPhraseLower"));
                    break;

                case "box":
                    cursor.SetSortOrder(orderDirection == "asc"
                        ? SortBy.Ascending("Box", "LanguageName", "State", "WordPhraseLower")
                        : SortBy.Descending("Box").Ascending("LanguageName", "State", "WordPhraseLower"));
                    break;

                case "word":
                    cursor.SetSortOrder(orderDirection == "asc"
                                            ? SortBy.Ascending("WordPhraseLower", "LanguageName", "State", "Box")
                                            : SortBy.Descending("WordPhraseLower").Ascending("LanguageName", "State", "Box"));
                    break;

                case "language":
                default:
                    cursor.SetSortOrder(orderDirection == "asc"
                                            ? SortBy.Ascending("LanguageName", "State", "WordPhraseLower", "Box")
                                            : SortBy.Descending("LanguageName").Ascending("State", "WordPhraseLower", "Box"));
                    break;
            }

            cursor.SetSkip((page - 1) * limit).SetLimit(limit);

            return new SearchResult<Word>()
            {
                Count = cursor.Count(),
                Items = cursor
            };
        }

        public IEnumerable<Word> FindSharedDefinitions(User user, string word, ObjectId systemLanguageId)
        {
            word = (word ?? "").ToLowerInvariant().Trim();
            var collection = _db.GetCollection<Word>(Collections.Words).AsQueryable();

            var userIds =
                user.ShareWords
                    ? _db.GetCollection<User>(Collections.Users)
                          .AsQueryable()
                          .Where(x => x.ShareWords)
                          .Select(x => x.UserId)
                    : _db.GetCollection<User>(Collections.Users)
                          .AsQueryable()
                          .Where(x => x.ShareWords && x.UserId == user.UserId)
                          .Select(x => x.UserId);

            var initial = collection.Where(x =>
                                           (x.WordPhraseLower == word || x.BaseWordLower == word) &&
                                           x.SystemLanguageId == systemLanguageId &&
                                           userIds.Contains(x.Owner)
                ).ToArray();

            var baseWords = initial.Select(x => x.BaseWordLower).Distinct().ToArray();

            var second = collection.Where(x =>
                                          (baseWords.Contains(x.WordPhraseLower) || baseWords.Contains(x.BaseWordLower)) &&
                                          x.SystemLanguageId == systemLanguageId &&
                                          userIds.Contains(x.Owner)
                );

            var union = initial.Union(second)
                .ToArray()
                .Distinct(new WordEqualityComparer())
                .Take(50)
                .OrderBy(x => x.WordPhraseLower);

            return union;
        }

        private class WordEqualityComparer : IEqualityComparer<Word>
        {
            public bool Equals(Word x, Word y)
            {
                return x.WordId == y.WordId;
            }

            public int GetHashCode(Word obj)
            {
                return obj.WordId.GetHashCode();
            }
        }
    }
}