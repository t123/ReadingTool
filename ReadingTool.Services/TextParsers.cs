#region License
// TextParsers.cs is part of ReadingTool.Services
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
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using ReadingTool.Entities;

namespace ReadingTool.Services
{
    public interface ITextParsers
    {
        IEnumerable<TextParser> FindAll();
        void Save(TextParser textParser);
        TextParser FindOne(string id);
        TextParser FindOne(ObjectId id);
        TextParser FindOne(ObjectId? id);
        void Delete(TextParser parser);
    }

    public class TextParsers : ITextParsers
    {
        private readonly MongoDatabase _db;

        public TextParsers(MongoDatabase db)
        {
            _db = db;
        }

        public IEnumerable<TextParser> FindAll()
        {
            return _db.GetCollection<TextParser>(Collections.TextParsers)
                .FindAll()
                .SetSortOrder(SortBy.Ascending("Name"));
        }

        public void Save(TextParser textParser)
        {
            if (textParser == null) return;
            _db.GetCollection(Collections.TextParsers).Save(textParser);
        }

        public TextParser FindOne(string id)
        {
            if(string.IsNullOrEmpty(id)) return null;
            return FindOne(new ObjectId(id));
        }

        public TextParser FindOne(ObjectId id)
        {
            return _db.GetCollection<TextParser>(Collections.TextParsers).FindOneById(id);
        }

        public TextParser FindOne(ObjectId? id)
        {
            if (id == null) return null;
            return FindOne(id.Value);
        }

        public void Delete(TextParser parser)
        {
            if(parser == null) return;
            _db.GetCollection(Collections.TextParsers).Remove(Query.EQ("_id", parser.TextParserId), RemoveFlags.Single);
        }
    }
}