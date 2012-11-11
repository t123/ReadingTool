#region License
// ItemService.cs is part of ReadingTool.Services
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
using System.Text;
using System.Text.RegularExpressions;
using FluentMongo.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using ReadingTool.Common.Enums;
using ReadingTool.Common.Helpers;
using ReadingTool.Common.Keys;
using ReadingTool.Entities;
using ReadingTool.Entities.Identity;
using ReadingTool.Entities.JSON;
using ReadingTool.Entities.Search;
using StructureMap;

namespace ReadingTool.Services
{
    public interface IItemService
    {
        void Save(Item item);
        Item FindOne(string id);
        Item FindOne(ObjectId id);
        void Delete(string id);
        void Delete(ObjectId id);
        IEnumerable<Item> FindAllForOwner();
        IEnumerable<Item> FindAllForOwner(ItemType? type);
        IEnumerable<Item> FindRecentlySeenForOwner();
        IEnumerable<Item> FindRecentlySeenForOwner(ItemType? type);
        IEnumerable<Item> FindAllForLanguage(ObjectId id);
        IEnumerable<Item> FindAllForLanguage(ItemType? type, ObjectId id);

        IEnumerable<string> AutocompleteCollectionName(string[] types, string[] languageIds);
        IEnumerable<string> AutocompleteCollectionName(string term);
        IEnumerable<string> AutocompleteCollectionName(ItemType? type, string term);
        IEnumerable<string> AutocompleteCollectionName(string term, int limit);
        IEnumerable<string> AutocompleteCollectionName(ItemType? type, string term, int limit);
        IEnumerable<string> AutocompleteTags(string term);
        IEnumerable<string> AutocompleteTags(ItemType? type, string term);
        IEnumerable<string> AutocompleteTags(string term, int limit);
        IEnumerable<string> AutocompleteTags(ItemType? type, string term, int limit);

        ObjectId NextItemId(Item currentItem);
        ObjectId PreviousItemId(Item currentItem);
        void MarkAsSeen(Item item);

        void SplitItem(string id, int? startingNumber, string tags);
        int ImportFromFile(IEnumerable<Language> languages, JsonTextFromFile json);
        SearchResult<Item> SearchItemsForGroup(ObjectId groupId, string filter, string[] folders, int limit, int page);
        Item FindOneForGroup(ObjectId textId, ObjectId groupId);
        SearchResult<Item> SearchItems(string[] types, string filter, string[] languages, string[] collectionNames, string orderBy, string orderDirection, int limit, int page);
        Tuple<long, IEnumerable<BsonDocument>> FindAllParsingTimes(int page);
        void DeleteAllParsingTimes();
    }

    public class ItemService : IItemService
    {
        private readonly MongoDatabase _db;
        private readonly UserForService _identity;
        private readonly ILanguageService _languageService;

        public ItemService(
            MongoDatabase db,
            UserForService identity,
            ILanguageService languageService
            )
        {
            _db = db;
            _identity = identity;
            _languageService = languageService;
        }

        #region autocomplete

        public IEnumerable<string> AutocompleteCollectionName(string[] types, string[] languageIds)
        {
            var queries = new List<IMongoQuery>();

            if(types != null && types.Length == 1)
            {
                if(types[0].Equals("texts", StringComparison.InvariantCultureIgnoreCase))
                {
                    queries.Add(Query.EQ("ItemType", ItemType.Text));
                }
                else
                {
                    queries.Add(Query.EQ("ItemType", ItemType.Video));
                }
            }

            queries.Add(Query.EQ("Owner", _identity.UserId));
            queries.Add(Query.NE("CollectionName", BsonNull.Value));

            if(languageIds != null && languageIds.Length > 0)
            {
                IList<ObjectId> ids = languageIds.Where(x => x.Length == 24).Select(x => ObjectId.Parse(x)).ToList();
                queries.Add(Query.In("LanguageId", new BsonArray(ids)));
            }

            var result =
                _db.GetCollection<string>(Collections.Items)
                .Distinct("CollectionName", Query.And(queries.ToArray()))
                ;

            return result.OrderBy(x => x).Take(10).Select(x => x.AsString).Where(x => x.Length > 0);
        }

        public IEnumerable<string> AutocompleteCollectionName(string term)
        {
            return AutocompleteCollectionName(term, 10);
        }

        public IEnumerable<string> AutocompleteCollectionName(ItemType? type, string term)
        {
            return AutocompleteCollectionName(type, term, 10);
        }

        public IEnumerable<string> AutocompleteCollectionName(string term, int limit)
        {
            return AutocompleteCollectionName(null, term, 10);
        }

        public IEnumerable<string> AutocompleteCollectionName(ItemType? type, string term, int limit)
        {
            QueryComplete query;

            if(type.HasValue)
            {
                query = Query.And(
                    Query.EQ("Owner", _identity.UserId),
                    Query.Matches("CollectionName", BsonRegularExpression.Create(new Regex(term, RegexOptions.IgnoreCase))),
                    Query.EQ("ItemType", type.Value)
                );
            }
            else
            {
                query = Query.And(
                    Query.EQ("Owner", _identity.UserId),
                    Query.Matches("CollectionName", BsonRegularExpression.Create(new Regex(term, RegexOptions.IgnoreCase)))
                );
            }

            var result =
                _db.GetCollection<Item>(Collections.Items)
                .Distinct("CollectionName", query)
                ;

            return limit < 0
                       ? result.OrderBy(x => x).Select(x => x.AsString)
                       : result.OrderBy(x => x).Take(limit).Select(x => x.AsString);
        }

        public IEnumerable<string> AutocompleteTags(string term)
        {
            return AutocompleteTags(term, 10);

        }

        public IEnumerable<string> AutocompleteTags(ItemType? type, string term)
        {
            return AutocompleteTags(type, term, 10);
        }

        public IEnumerable<string> AutocompleteTags(string term, int limit)
        {
            return AutocompleteTags(null, term, 10);
        }

        public IEnumerable<string> AutocompleteTags(ItemType? type, string term, int limit)
        {
            //TODO fixme to filter in DB
            QueryComplete query = type == null
                                      ? Query.EQ("Owner", _identity.UserId)
                                      : Query.And(Query.EQ("Owner", _identity.UserId), Query.EQ("ItemType", type.Value))
                ;

            var results =
                _db.GetCollection<Item>(Collections.Items)
                .Distinct("Tags", query)
                .Select(x => x.AsString)
                .Where(x => x.StartsWith(term, StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(x => x);

            return limit < 0 ? results : results.Take(limit);
        }
        #endregion

        #region usuful stuff
        public ObjectId NextItemId(Item currentItem)
        {
            if(currentItem == null) return ObjectId.Empty;
            if(string.IsNullOrEmpty(currentItem.CollectionName)) return ObjectId.Empty;
            if(!currentItem.CollectionNo.HasValue) return ObjectId.Empty;

            var query = Query.And(
                Query.EQ("Owner", _identity.UserId),
                Query.EQ("LanguageId", currentItem.LanguageId),
                Query.EQ("CollectionName", currentItem.CollectionName),
                Query.GT("CollectionNo", currentItem.CollectionNo),
                Query.EQ("ItemType", currentItem.ItemType)
                );

            return
                _db.GetCollection<Item>(Collections.Items)
                .Find(query)
                .SetSortOrder("CollectionNo")
                .SetLimit(1)
                .SetFields("_id")
                .Select(x => x.ItemId)
                .FirstOrDefault()
                ;
        }

        public ObjectId PreviousItemId(Item currentItem)
        {
            if(currentItem == null) return ObjectId.Empty;
            if(string.IsNullOrEmpty(currentItem.CollectionName)) return ObjectId.Empty;
            if(!currentItem.CollectionNo.HasValue) return ObjectId.Empty;

            var query = Query.And(
                Query.EQ("Owner", _identity.UserId),
                Query.EQ("LanguageId", currentItem.LanguageId),
                Query.EQ("CollectionName", currentItem.CollectionName),
                Query.LT("CollectionNo", currentItem.CollectionNo),
                Query.EQ("ItemType", currentItem.ItemType)
                );

            return
                _db.GetCollection<Item>(Collections.Items)
                .Find(query)
                .SetSortOrder(SortBy.Descending("CollectionNo"))
                .SetLimit(1)
                .SetFields("_id")
                .Select(x => x.ItemId)
                .FirstOrDefault()
                ;
        }

        public void MarkAsSeen(Item item)
        {
            if(item == null) return;
            _db.GetCollection(Collections.Items)
                .Update
                (
                    Query.And
                        (
                            Query.EQ("_id", item.ItemId),
                            Query.EQ("Owner", _identity.UserId)
                        ),
                    Update.Set("LastSeen", DateTime.Now)
                );
        }
        #endregion

        #region basic
        public void Save(Item item)
        {
            if(item.ItemId == ObjectId.Empty)
            {
                item.Created = DateTime.Now;
                item.Owner = _identity.UserId;
            }

            if(
                string.IsNullOrEmpty(item.LanguageName) ||
                string.IsNullOrEmpty(item.LanguageColour) ||
                item.SystemLanguageId == ObjectId.Empty
                )
            {
                var languageService = ObjectFactory.GetInstance<ILanguageService>();
                var language = languageService.FindOne(item.LanguageId);

                if(language != null)
                {
                    item.LanguageName = language.Name;
                    item.LanguageColour = language.Colour;
                    item.SystemLanguageId = language.SystemLanguageId;
                }
            }

            item.Modified = DateTime.Now;
            _db.GetCollection(Collections.Items).Save(item);
        }

        public Item FindOne(string id)
        {
            if(id == null) return null;
            return FindOne(new ObjectId(id));
        }

        public Item FindOne(ObjectId id)
        {
            return _db.GetCollection<Item>(Collections.Items)
                .AsQueryable()
                .FirstOrDefault(x => x.Owner == _identity.UserId && x.ItemId == id);
        }

        public void Delete(string id)
        {
            Delete(new ObjectId(id));
        }

        public void Delete(ObjectId id)
        {
            _db.GetCollection<Item>(Collections.Items).Remove(
                Query.And(
                    Query.EQ("Owner", _identity.UserId),
                    Query.EQ("_id", id)
                    )
                );
        }

        public IEnumerable<Item> FindAllForOwner()
        {
            return FindAllForOwner(null);
        }

        public IEnumerable<Item> FindAllForOwner(ItemType? type)
        {
            var query =
                _db.GetCollection<Item>(Collections.Items)
                .AsQueryable()
                .Where(x => x.Owner == _identity.UserId);

            if(type != null)
                query = query.Where(x => x.ItemType == type);

            return query
                .OrderBy(x => x.CollectionName)
                .ThenBy(x => x.CollectionNo)
                .ThenBy(x => x.Title)
                ;
        }

        public IEnumerable<Item> FindRecentlySeenForOwner()
        {
            return FindRecentlySeenForOwner(null);
        }

        public IEnumerable<Item> FindRecentlySeenForOwner(ItemType? type)
        {
            var query =
                _db.GetCollection<Item>(Collections.Items)
                .AsQueryable()
                .Where(x => x.Owner == _identity.UserId);

            if(type != null)
                query = query.Where(x => x.ItemType == type);

            return query
                .OrderByDescending(x => x.LastSeen)
                .Take(15)
                ;
        }

        public IEnumerable<Item> FindAllForLanguage(ObjectId id)
        {
            return FindAllForLanguage(null, id);
        }

        public IEnumerable<Item> FindAllForLanguage(ItemType? type, ObjectId id)
        {
            var query =
                _db.GetCollection<Item>(Collections.Items)
                .AsQueryable()
                .Where(x => x.Owner == _identity.UserId && x.LanguageId == id);

            if(type != null)
                query = query.Where(x => x.ItemType == type);

            return query
                .OrderBy(x => x.CollectionName)
                .ThenBy(x => x.CollectionNo)
                .ThenBy(x => x.Title)
                ;
        }
        #endregion

        #region other stuff
        public void SplitItem(string id, int? startingNumber, string tags)
        {
            var item = FindOne(id);

            if(item == null)
            {
                throw new NoNullAllowedException("Text cannot be null");
            }

            string[] pieces = item.L1Text.Split(new[] { "===" }, StringSplitOptions.None).Select(x => x.Trim()).ToArray();
            string[] parallelPieces = item.L2Text.Split(new[] { "===" }, StringSplitOptions.None).Select(x => x.Trim()).ToArray();

            int? currentNo = startingNumber;
            for(int i = 0; i < pieces.Length; i++)
            {
                var piece = pieces[i];
                var newItem = item.Clone();

                newItem.ItemId = ObjectId.Empty;
                newItem.L1Text = piece;
                newItem.L2Text = item.IsParallel ? parallelPieces[i] : ""; //TODO check index of array here
                newItem.CollectionNo = currentNo;
                newItem.LastSeen = null;

                if(currentNo.HasValue) currentNo++;

                Save(newItem);
            }
        }

        public int ImportFromFile(IEnumerable<Language> languages, JsonTextFromFile json)
        {
            JsonDefaults defaults = new JsonDefaults();
            StringBuilder errors = new StringBuilder();

            if(json.Defaults != null)
            {
                if(!string.IsNullOrEmpty(json.Defaults.Language))
                {
                    var language = languages.FirstOrDefault(x => x.Name.Equals(json.Defaults.Language, StringComparison.InvariantCultureIgnoreCase));

                    if(language == null)
                    {
                        errors.AppendFormat("The language '<b>{0}</b>' was not found in the default settings<br/>", json.Defaults.Language);
                    }
                    else
                    {
                        defaults.LanguageId = language.LanguageId;
                    }
                }

                defaults.AutoNumberCollection = json.Defaults.AutoNumberCollection;
                defaults.CollectionName = json.Defaults.CollectionName;
                defaults.StartCollectionWithNumber = json.Defaults.StartCollectionWithNumber;
                defaults.Tags = json.Defaults.Tags;
                defaults.ShareUrl = json.Defaults.ShareUrl;
                defaults.ItemType = json.Defaults.ItemType;
            }

            int counter = 1;
            foreach(var item in json.Items)
            {
                var language = languages.FirstOrDefault(x => x.Name.Equals(item.Language, StringComparison.InvariantCultureIgnoreCase));

                if(language == null && defaults.LanguageId == ObjectId.Empty)
                {
                    if(string.IsNullOrEmpty(item.Language))
                    {
                        errors.AppendFormat("The language '<b>{0}</b>' was not found in the text items, and no default was specified for item {1}<br/>", item.Language ?? "none", counter);
                    }
                    else
                    {
                        errors.AppendFormat("No language or default was specified for item {0}<br/>", counter);
                    }
                }

                if(item.ItemType == null && defaults.ItemType == null)
                {
                    errors.AppendFormat("No type or default was specified for item {0}. Use 0 for texts and 1 for videos<br/>", counter);
                }

                item.LanguageId = language == null ? ObjectId.Empty : language.LanguageId;
                item.LanguageName = language == null ? "" : language.Name;
                item.LanguageColour = language == null ? "" : language.Colour;
                item.ItemType = item.ItemType ?? defaults.ItemType;

                if(string.IsNullOrEmpty(item.Title))
                {
                    errors.AppendFormat(string.Format("The title was not specified for item {0}<br/>", counter));
                }

                if(string.IsNullOrEmpty(item.L1Text))
                {
                    errors.AppendFormat(string.Format("The text was not specified for item {0}<br/>", counter));
                }

                counter++;
            }

            if(errors.Length > 0)
            {
                throw new Exception(errors.ToString());
            }

            int imported = 0;
            int currentCollectionNo = (defaults.AutoNumberCollection ?? false)
                                            ? defaults.StartCollectionWithNumber ?? 1
                                            : 1;

            foreach(var item in json.Items)
            {
                Item newItem = new Item
                {
                    Url = item.Url,
                    CollectionName = string.IsNullOrEmpty(item.CollectionName) ? defaults.CollectionName : item.CollectionName,
                    Owner = _identity.UserId,
                    LanguageId = item.LanguageId == ObjectId.Empty ? defaults.LanguageId : item.LanguageId,
                    LanguageName = item.LanguageName ?? "",
                    LanguageColour = item.LanguageColour ?? "",
                    L1Text = item.L1Text,
                    L2Text = item.L2Text,
                    Title = item.Title ?? "Import " + imported,
                    Tags = TagHelper.Merge(defaults.Tags, item.Tags),
                    ParallelIsRtl = item.ParallelIsRtl ?? false,
                    ItemType = item.ItemType.Value
                };

                if(item.CollectionNo != null)
                {
                    newItem.CollectionNo = item.CollectionNo;
                }
                else
                {
                    if(defaults.AutoNumberCollection ?? false)
                    {
                        newItem.CollectionNo = currentCollectionNo;
                        currentCollectionNo++;
                    }
                }

                Save(newItem);

                imported++;
            }

            return imported;
        }

        public SearchResult<Item> SearchItemsForGroup(ObjectId groupId, string filter, string[] folders, int limit, int page)
        {
            filter = (filter ?? "").ToLowerInvariant().Trim();
            var texts = _db.GetCollection<Item>(Collections.Items);

            var queries = new List<IMongoQuery>() { Query.In("GroupId", groupId) };

            if(folders != null && folders.Length == 1)
            {
                queries.Add(Query.EQ("ItemType", folders[0].Equals("Texts", StringComparison.InvariantCultureIgnoreCase) ? ItemType.Text : ItemType.Video));
            }

            string[] splitFilter = filter.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string[] tags = splitFilter.Where(x => x.StartsWith("#")).Select(x => x.Substring(1, x.Length - 1)).ToArray();
            filter = string.Join(" ", splitFilter.Where(x => !x.StartsWith("#")));

            if(tags.Length > 0)
                queries.Add(Query.In("Tags", new BsonArray(tags)));

            if(!string.IsNullOrEmpty(filter))
            {
                queries.Add(Query.Matches("TitleLower", BsonRegularExpression.Create(new Regex(filter))));
            }

            var cursor = texts.Find(Query.And(queries.ToArray()));

            cursor.SetSortOrder(SortBy.Ascending("SystemLanguageId", "CollectionName", "CollectionNo", "Title"));
            cursor.SetSkip((page - 1) * limit).SetLimit(limit);

            return new SearchResult<Item>()
            {
                Count = cursor.Count(),
                Items = cursor
            };
        }

        public Item FindOneForGroup(ObjectId itemId, ObjectId groupId)
        {
            return _db.GetCollection<Item>(Collections.Items)
                .AsQueryable()
                .FirstOrDefault(x => x.ItemId == itemId && x.GroupId.Contains(groupId));
        }

        public SearchResult<Item> SearchItems(string[] types, string filter, string[] languages, string[] collectionNames, string orderBy, string orderDirection, int limit, int page)
        {
            var texts = _db.GetCollection<Item>(Collections.Items);

            var model = FilterParser.ParseItems(_languageService.FindAll().Select(x => x.Name.ToLowerInvariant()).ToArray(), filter);

            IMongoQuery query = Query.EQ("Owner", _identity.UserId);
            IList<IMongoQuery> lQueries = new List<IMongoQuery>();
            IList<IMongoQuery> oQueries = new List<IMongoQuery>();
            IMongoQuery tagQuery = new QueryDocument();

            if(model.Languages.Count > 0)
            {
                foreach(var l in model.Languages)
                {
                    lQueries.Add(Query.EQ("LanguageNameLower", l));
                }
            }

            if(languages != null && !(languages.Length == 1 && languages[0].Equals("")))
            {
                lQueries.Add(Query.In("LanguageId", new BsonArray(languages.Select(x => new ObjectId(x)))));
            }

            if(model.Tags.Count > 0)
            {
                tagQuery = Query.In("Tags", new BsonArray(model.Tags));
            }

            if(model.Other.Count > 0)
            {
                foreach(var o in model.Other)
                {
                    oQueries.Add(Query.Or(
                        Query.Matches("TitleLower", BsonRegularExpression.Create(new Regex(o))),
                        Query.Matches("CollectionNameLower", BsonRegularExpression.Create(new Regex(o)))
                    ));
                }
            }

            if(collectionNames != null && !(collectionNames.Length == 1 && collectionNames[0].Equals("")))
            {
                int indexNone = Array.IndexOf(collectionNames, "(none)");

                if(indexNone >= 0)
                {
                    collectionNames[indexNone] = "";
                }

                foreach(var o in collectionNames)
                {
                    oQueries.Add(Query.Or(Query.Matches("CollectionNameLower", BsonRegularExpression.Create(new Regex(o.ToLowerInvariant())))));
                }
            }

            query = Query.And(query, tagQuery);

            if(types != null && types.Length == 1)
            {
                query = Query.And(query, Query.EQ("ItemType", types[0].Equals("Texts", StringComparison.InvariantCultureIgnoreCase) ? ItemType.Text : ItemType.Video));
            }

            if(lQueries.Count > 0) query = Query.And(query, Query.Or(lQueries.ToArray()));
            if(oQueries.Count > 0) query = Query.And(query, Query.Or(oQueries.ToArray()));

            MongoCursor<Item> cursor = texts.Find(query).SetSortOrder("ItemType");
            //var cursor = texts.Find(Query.And(queries.ToArray())).SetSortOrder("ItemType");

            switch(orderBy)
            {
                case "lastseen":
                    cursor.SetSortOrder(orderDirection == "asc"
                        ? SortBy.Descending("LastSeen").Ascending("LanguageName", "CollectionName", "CollectionNo", "Title")
                        : SortBy.Ascending("LastSeen").Ascending("LanguageName", "CollectionName", "CollectionNo", "Title"));
                    break;

                case "collection":
                    cursor.SetSortOrder(orderDirection == "asc"
                        ? SortBy.Ascending("CollectionName", "CollectionNo", "Title")
                        : SortBy.Descending("CollectionName").Ascending("CollectionNo", "Title"));
                    break;

                case "title":
                    cursor.SetSortOrder(orderDirection == "asc"
                                            ? SortBy.Ascending("Title", "CollectionName", "CollectionNo")
                                            : SortBy.Descending("Title").Ascending("CollectionName", "CollectionNo"));
                    break;

                case "language":
                default:
                    cursor.SetSortOrder(orderDirection == "asc"
                                            ? SortBy.Ascending("LanguageName", "CollectionName", "CollectionNo", "Title")
                                            : SortBy.Descending("LanguageName").Ascending("CollectionName", "CollectionNo", "Title"));
                    break;
            }

            cursor.SetFields(
                "LanguageName", "LanguageColour", "Title", "CollectionName", "CollectionNo",
                "_id", "LastSeen", "IsParallel", "Url", "GroupId", "ItemType");
            cursor.SetSkip((page - 1) * limit).SetLimit(limit);

            return new SearchResult<Item>()
            {
                Count = cursor.Count(),
                Items = cursor
            };
        }

        public Tuple<long, IEnumerable<BsonDocument>> FindAllParsingTimes(int page)
        {
            var cursor = _db.GetCollection(Collections.ParsingTimes)
                .FindAll()
                .SetSortOrder(SortBy.Descending("Created"))
                .SetSkip((page - 1) * 20).SetLimit(20)
                ;

            return new Tuple<long, IEnumerable<BsonDocument>>(
                cursor.Count(),
                cursor
                );
        }

        public void DeleteAllParsingTimes()
        {
            _db.GetCollection(Collections.ParsingTimes).RemoveAll();
        }

        #endregion
    }
}