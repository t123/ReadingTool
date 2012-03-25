#region License
// LogService.cs is part of ReadingTool.Services
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
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace ReadingTool.Services
{
    public interface ILogService
    {
        Tuple<long, IEnumerable<BsonDocument>> FindAll(int page);
        void DeleteAll();
    }

    public class LogService : ILogService
    {
        private readonly MongoDatabase _db;

        public LogService(MongoDatabase db)
        {
            _db = db;
        }

        public Tuple<long, IEnumerable<BsonDocument>> FindAll(int page)
        {
            var cursor = _db.GetCollection(Collections.Logs)
                .FindAll()
                .SetSortOrder(SortBy.Descending("timestamp"))
                .SetSkip((page - 1) * 20).SetLimit(20)
                ;

            return new Tuple<long, IEnumerable<BsonDocument>>(
                cursor.Count(),
                cursor
                );
        }

        public void DeleteAll()
        {
            _db.GetCollection(Collections.Logs).RemoveAll();
        }
    }
}
