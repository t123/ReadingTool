#region License
// LanguageService.cs is part of ReadingTool.Services
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
using System.Linq;
using FluentMongo.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using ReadingTool.Entities;
using ReadingTool.Entities.Identity;

namespace ReadingTool.Services
{
    public interface ILanguageService
    {
        void Save(Language language);
        IEnumerable<Language> FindAll();
        Language FindOne(string id);
        Language FindOne(ObjectId id);
        IEnumerable<Language> FindAllForOwner();
        void UpdateDictionary(Language language, string name, UserDictionary dictionary);
        void DeleteDictionary(Language language, string name);
        void Delete(string id);
        void Delete(ObjectId id);

        IEnumerable<Language> FindAllForUser(ObjectId objectId);
        IEnumerable<Language> FindLanguages(IList<ObjectId> languageIds);
        Language FindOneForUserBySystemLanguage(ObjectId userId, ObjectId systemLanguageId);
    }

    public class LanguageService : ILanguageService
    {
        private readonly MongoDatabase _db;
        private readonly UserForService _identity;

        public LanguageService(
            MongoDatabase db,
            UserForService identity
            )
        {
            _db = db;
            _identity = identity;
        }

        public void Save(Language language)
        {
            if(language.LanguageId == ObjectId.Empty)
            {
                language.Created = DateTime.Now;
                language.Owner = _identity.UserId;
            }

            language.Modified = DateTime.Now;
            _db.GetCollection(Collections.Languages).Save(language);

            _db.GetCollection(Collections.Items)
                .Update
                (
                    Query.And
                        (
                            Query.EQ("Owner", _identity.UserId),
                            Query.EQ("LanguageId", language.LanguageId)
                        ),
                    Update
                        .Set("LanguageName", language.Name)
                        .Set("LanguageColour", language.Colour)
                        .Set("SystemLanguageId", language.SystemLanguageId),
                    UpdateFlags.Multi
                );

            _db.GetCollection(Collections.Words)
                .Update
                (
                    Query.And
                        (
                            Query.EQ("Owner", _identity.UserId),
                            Query.EQ("LanguageId", language.LanguageId)
                        ),
                    Update
                        .Set("LanguageName", language.Name)
                        .Set("LanguageColour", language.Colour)
                        .Set("SystemLanguageId", language.SystemLanguageId),
                    UpdateFlags.Multi
                );
        }

        public IEnumerable<Language> FindAll()
        {
            return _db.GetCollection<Language>(Collections.Languages)
                .AsQueryable()
                .OrderBy(x => x.Name);
        }

        public Language FindOne(string id)
        {
            return FindOne(new ObjectId(id));
        }

        public Language FindOne(ObjectId id)
        {
            return _db.GetCollection<Language>(Collections.Languages)
                .AsQueryable()
                .FirstOrDefault(x => x.Owner == _identity.UserId && x.LanguageId == id);
        }

        public IEnumerable<Language> FindAllForOwner()
        {
            return _db.GetCollection<Language>(Collections.Languages)
                .AsQueryable()
                .Where(x => x.Owner == _identity.UserId)
                .OrderBy(x => x.Name);
        }

        public void UpdateDictionary(Language language, string name, UserDictionary dictionary)
        {
            var collection = _db.GetCollection<Language>(Collections.Languages);
            var query = Query.And(
                Query.EQ("_id", language.LanguageId),
                Query.EQ("Dictionaries.Name", name)
                );

            UpdateBuilder builder = Update.Set("Dictionaries.$.Name", dictionary.Name);
            builder.Combine(Update.Set("Dictionaries.$.WindowName", dictionary.WindowName));
            builder.Combine(Update.Set("Dictionaries.$.Url", dictionary.Url));

            collection.Update(query, builder);
        }

        public void DeleteDictionary(Language language, string name)
        {
            _db.GetCollection<Language>(Collections.Languages).Update(
                    Query.EQ("_id", language.LanguageId),
                    Update.Pull("Dictionaries", Query.EQ("Name", name))
            );
        }

        public void Delete(string id)
        {
            Delete(new ObjectId(id));
        }

        public void Delete(ObjectId id)
        {
            _db.GetCollection(Collections.Items)
                .Remove(
                    Query.And(
                        Query.EQ("Owner", _identity.UserId),
                        Query.EQ("LanguageId", id)
                    )
                );

            _db.GetCollection(Collections.Languages)
                .Remove(
                    Query.And(
                        Query.EQ("Owner", _identity.UserId),
                        Query.EQ("_id", id)
                    )
                );
        }

        public IEnumerable<Language> FindAllForUser(ObjectId userId)
        {
            return _db.GetCollection<Language>(Collections.Languages)
                .AsQueryable()
                .Where(x => x.Owner == userId)
                .OrderBy(x => x.Name);
        }

        public IEnumerable<Language> FindLanguages(IList<ObjectId> languageIds)
        {
            return _db.GetCollection<Language>(Collections.Languages)
                .Find(Query.In("_id", new BsonArray(languageIds)));
        }

        public Language FindOneForUserBySystemLanguage(ObjectId userId, ObjectId systemLanguageId)
        {
            return _db.GetCollection<Language>(Collections.Languages)
                .AsQueryable()
                .FirstOrDefault(x => x.SystemLanguageId == systemLanguageId && x.Owner == userId);
        }
    }
}