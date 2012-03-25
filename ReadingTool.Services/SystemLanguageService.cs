#region License
// SystemLanguageService.cs is part of ReadingTool.Services
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

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentMongo.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using ReadingTool.Entities;

namespace ReadingTool.Services
{
    public interface ISystemLanguageService
    {
        void Save(SystemLanguage language);
        IEnumerable<SystemLanguage> AutocompleteName(string name);
        SystemLanguage FindOne(ObjectId systemLanguageId);
        IEnumerable<SystemLanguage> FindAll();
        SystemLanguage FindByCode(string code);
        SystemLanguage FindByName(string name);
        void Save(IList<SystemLanguage> languages);
        IEnumerable<SystemLanguage> FindAllInUse();
        IEnumerable<SystemLanguage> FindAll(IEnumerable<ObjectId> languageIds);
    }

    public class SystemLanguageService : ISystemLanguageService
    {
        private readonly MongoDatabase _db;

        public SystemLanguageService(MongoDatabase db)
        {
            _db = db;
        }

        public void Save(SystemLanguage language)
        {
            _db.GetCollection(Collections.SystemLanguages).Save(language);
        }

        public IEnumerable<SystemLanguage> AutocompleteName(string name)
        {
            var lowered = (name ?? "").ToLowerInvariant();
            return
                _db.GetCollection<SystemLanguage>(Collections.SystemLanguages)
                    .Find(
                        Query.And(
                            Query.NE("Code", SystemLanguage.NotYetSetCode),
                            Query.Matches("LowerName", BsonRegularExpression.Create(new Regex(lowered)))
                            )
                    )
                    .SetSortOrder(SortBy.Ascending("LowerName"))
                    .SetLimit(15);
        }

        public SystemLanguage FindOne(ObjectId systemLanguageId)
        {
            return _db.GetCollection<SystemLanguage>(Collections.SystemLanguages).FindOneById(systemLanguageId);
        }

        public IEnumerable<SystemLanguage> FindAll()
        {
            return _db.GetCollection<SystemLanguage>(Collections.SystemLanguages).FindAll();
        }

        public SystemLanguage FindByCode(string code)
        {
            return _db.GetCollection<SystemLanguage>(Collections.SystemLanguages)
                .AsQueryable()
                .FirstOrDefault(x => x.Code == code);
        }

        public SystemLanguage FindByName(string name)
        {
            return _db.GetCollection<SystemLanguage>(Collections.SystemLanguages)
                .AsQueryable()
                .FirstOrDefault(x => x.LowerName == name.ToLowerInvariant());
        }

        public void Save(IList<SystemLanguage> languages)
        {
            _db.GetCollection(Collections.SystemLanguages).InsertBatch(languages);
        }

        public IEnumerable<SystemLanguage> FindAllInUse()
        {
            var ids = _db.GetCollection(Collections.Languages)
                .Distinct("SystemLanguageId");

            return _db.GetCollection<SystemLanguage>(Collections.SystemLanguages)
                .Find(Query.In("_id", ids))
                .SetSortOrder(SortBy.Ascending("Name"));
        }

        public IEnumerable<SystemLanguage> FindAll(IEnumerable<ObjectId> languageIds)
        {
            return _db.GetCollection<SystemLanguage>(Collections.SystemLanguages)
                .Find(Query.In("_id", new BsonArray(languageIds)))
                .SetSortOrder(SortBy.Ascending("Name"));
        }
    }
}