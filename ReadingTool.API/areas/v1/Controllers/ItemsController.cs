#region License
// ItemsController.cs is part of ReadingTool.API
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
using ReadingTool.API.Areas.V1.Models;
using ReadingTool.API.Common;
using ReadingTool.Entities;
using ReadingTool.Services;

namespace ReadingTool.API.Areas.V1.Controllers
{
    public class ItemsController : BaseController
    {
        public JsonNetResult Single(string itemId)
        {
            var data = _db.GetCollection<Item>(Collections.Items)
                .AsQueryable()
                .FirstOrDefault(x => x.Owner == _userId && x.ItemId == new ObjectId(itemId));

            var mapped = Mapper.Map<Item, ItemModel>(data);
            var reply = new ItemResponse()
            {
                Items = new ItemModel[] { mapped }
            };

            return new JsonNetResult() { Data = reply };
        }

        public JsonNetResult List()
        {
            if(Request.QueryString["count"] != null)
            {
                var count = _db.GetCollection<Item>(Collections.Items)
                .AsQueryable()
                .Count(x => x.Owner == _userId);

                return new JsonNetResult() { Data = new CountResponse() { Count = count } };
            }

            var data = _db.GetCollection<Item>(Collections.Items)
                .AsQueryable()
                .Where(x => x.Owner == _userId)
                .OrderBy(x => x.LanguageId)
                .ThenBy(x => x.CollectionName)
                .ThenBy(x => x.CollectionNo)
                .ThenBy(x => x.Title)
                .Skip((_page - 1) * _maxItemsPerPage)
                .Take(_maxItemsPerPage)
                ;

            var mapped = Mapper.Map<IEnumerable<Item>, IEnumerable<ItemModel>>(data);
            var reply = new ItemResponse()
            {
                Items = mapped.ToArray()
            };

            return new JsonNetResult() { Data = reply };
        }

        public JsonNetResult ListByLanguage(string languageId)
        {
            if(Request.QueryString["count"] != null)
            {
                var count = _db.GetCollection<Item>(Collections.Items)
                .AsQueryable()
                .Count(x => x.Owner == _userId && x.LanguageId == new ObjectId(languageId));

                return new JsonNetResult() { Data = new CountResponse() { Count = count } };
            }

            var data = _db.GetCollection<Item>(Collections.Items)
                .AsQueryable()
                .Where(x => x.Owner == _userId && x.LanguageId == new ObjectId(languageId))
                .OrderBy(x => x.CollectionName)
                .ThenBy(x => x.CollectionNo)
                .ThenBy(x => x.Title)
                .Skip((_page - 1) * _maxItemsPerPage)
                .Take(_maxItemsPerPage)
                ;

            var mapped = Mapper.Map<IEnumerable<Item>, IEnumerable<ItemModel>>(data);
            var reply = new ItemResponse()
            {
                Items = mapped.ToArray()
            };

            return new JsonNetResult() { Data = reply };
        }

        public JsonNetResult ListByModified(string modified)
        {
            DateTime dt = DateTime.ParseExact(modified, "yyyyMMddHHmmss", null);

            if(Request.QueryString["count"] != null)
            {
                var count = _db.GetCollection<Item>(Collections.Items)
                .AsQueryable()
                .Count(x => x.Owner == _userId && x.Modified >= dt);

                return new JsonNetResult() { Data = new CountResponse() { Count = count } };
            }

            var data = _db.GetCollection<Item>(Collections.Items)
                 .AsQueryable()
                 .Where(x => x.Owner == _userId)
                 .OrderBy(x => x.Modified)
                 .Skip((_page - 1) * _maxItemsPerPage)
                 .Take(_maxItemsPerPage)
                 ;

            var mapped = Mapper.Map<IEnumerable<Item>, IEnumerable<ItemModel>>(data);
            var reply = new ItemResponse()
            {
                Items = mapped.ToArray()
            };

            return new JsonNetResult() { Data = reply };
        }

        public JsonNetResult ListByLanguageModified(string languageId, string modified)
        {
            DateTime dt = DateTime.ParseExact(modified, "yyyyMMddHHmmss", null);

            if(Request.QueryString["count"] != null)
            {
                var count = _db.GetCollection<Item>(Collections.Items)
                .AsQueryable()
                .Count(x => x.Owner == _userId && x.Modified >= dt && x.LanguageId == new ObjectId(languageId));

                return new JsonNetResult() { Data = new CountResponse() { Count = count, PerPage = _maxItemsPerPage } };
            }

            var data = _db.GetCollection<Item>(Collections.Items)
                .AsQueryable()
                .Where(x => x.Owner == _userId && x.Modified >= dt && x.LanguageId == new ObjectId(languageId))
                .OrderBy(x => x.Modified)
                .Skip((_page - 1) * _maxItemsPerPage)
                .Take(_maxItemsPerPage)
                ;

            var mapped = Mapper.Map<IEnumerable<Item>, IEnumerable<ItemModel>>(data);
            var reply = new ItemResponse()
            {
                Items = mapped.ToArray()
            };

            return new JsonNetResult() { Data = reply };
        }
    }
}
