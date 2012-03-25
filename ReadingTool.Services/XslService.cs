#region License
// XslService.cs is part of ReadingTool.Services
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
using ReadingTool.Common.Enums;
using ReadingTool.Entities;

namespace ReadingTool.Services
{
    public interface IXslService
    {
        void Save(Xsl xsl);
        string XslForItem(ObjectId systemLanguageId, ItemType type, bool parallel);
        IEnumerable<Xsl> FindAll();
    }

    public class XslService : IXslService
    {
        private readonly MongoDatabase _db;

        public XslService(MongoDatabase db)
        {
            _db = db;
        }

        public void Save(Xsl xsl)
        {
            if(xsl.XslId == ObjectId.Empty)
            {
            }

            _db.GetCollection(Collections.Xsl).Save(xsl);
        }

        public string XslForItem(ObjectId systemLanguageId, ItemType type, bool parallel)
        {
            var xsl = FindXsl(systemLanguageId, type, parallel) ?? FindXsl(ObjectId.Empty, type, parallel);

            if(xsl != null)
                return xsl.XslTransform;

            throw new NotSupportedException(string.Format("Could not find XSL for languageId {0}/{1}/{2}; no default found", systemLanguageId, type, parallel));
        }

        private Xsl FindXsl(ObjectId systemLanguageId, ItemType type, bool parallel)
        {
            var collection = _db.GetCollection<Xsl>(Collections.Xsl);

            if(type == ItemType.Text)
            {
                if(parallel)
                {
                    return collection.AsQueryable().FirstOrDefault(x => x.SystemLanguageId == systemLanguageId && x.Name == "Parallel");
                }
                else
                {
                    return collection.AsQueryable().FirstOrDefault(x => x.SystemLanguageId == systemLanguageId && x.Name == "Single");
                }
            }
            else
            {
                return collection.AsQueryable().FirstOrDefault(x => x.SystemLanguageId == systemLanguageId && x.Name == "Watch");
            }
        }

        public IEnumerable<Xsl> FindAll()
        {
            return _db.GetCollection<Xsl>(Collections.Xsl)
                .FindAll();
        }
    }
}
