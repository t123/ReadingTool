#region License
// MessageService.cs is part of ReadingTool.Services
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
using ReadingTool.Common.Enums;
using ReadingTool.Common.Keys;
using ReadingTool.Entities;
using ReadingTool.Entities.Identity;

namespace ReadingTool.Services
{
    public interface IMessageService
    {
        Message FindOne(string id);
        Message FindOne(ObjectId id);
        void Send(IList<ObjectId> to, string subject, string body);
        bool Star(string id, bool status);
        void Delete(string id);

        MongoCursor<Message> SearchIn(string[] folders, string filter);
        MongoCursor<Message> SearchOut(string[] folders, string filter);
        int UnreadMessageCount();
    }

    public class MessageService : IMessageService
    {
        private readonly MongoDatabase _db;
        private readonly UserForService _identity;

        public MessageService(
            MongoDatabase db,
            UserForService identity
            )
        {
            _db = db;
            _identity = identity;
        }

        public Message FindOne(string id)
        {
            return FindOne(new ObjectId(id));
        }

        public Message FindOne(ObjectId id)
        {
            _db.GetCollection(Collections.Messages)
                .FindAndModify(
                    Query.And(
                        Query.EQ("_id", id),
                        Query.EQ("Owner", _identity.UserId)
                    ),
                    null,
                    Update.Set("IsRead", true)
                );

            return _db.GetCollection<Message>(Collections.Messages)
                .AsQueryable()
                .FirstOrDefault(x => x.MessageId == id && (x.Owner == _identity.UserId || x.From == id));
        }

        public void Send(Message message)
        {
            if(message.MessageId == ObjectId.Empty)
            {
                message.Created = DateTime.Now;
                message.IsRead = message.IsStarred = false;
            }

            _db.GetCollection(Collections.Messages).Save(message);
        }

        public void Send(IList<ObjectId> to, string subject, string body)
        {
            IList<Message> messages = new List<Message>();

            messages.Add(new Message()
            {
                Body = body,
                To = to,
                Owner = _identity.UserId,
                Subject = subject,
                MessageType = MessageType.Sent,
                From = ObjectId.Empty,
                IsRead = true,
                Created = DateTime.Now
            });

            foreach(var id in to)
            {
                messages.Add(new Message
                                 {
                                     Body = body,
                                     From = _identity.UserId,
                                     Owner = id,
                                     Subject = subject,
                                     MessageType = MessageType.Received,
                                     To = null,
                                     Created = DateTime.Now
                                 });
            }

            _db.GetCollection(Collections.Messages).InsertBatch(messages);
        }

        public bool Star(string id, bool status)
        {
            _db.GetCollection(Collections.Messages).Update(
                Query.And(
                    Query.EQ("Owner", _identity.UserId),
                    Query.EQ("_id", new ObjectId(id))
                ), Update.Set("IsStarred", status)
            );
            return status;
        }

        public void Delete(string id)
        {
            _db.GetCollection(Collections.Messages).Remove(Query.And(Query.EQ("Owner", _identity.UserId), Query.EQ("_id", new ObjectId(id))));
        }

        public MongoCursor<Message> SearchIn(string[] folders, string filter)
        {
            var messages = _db.GetCollection<Message>(Collections.Messages);

            var queries = new List<IMongoQuery>()
                              {
                                  Query.And
                                  (
                                    Query.EQ("Owner", _identity.UserId),
                                    Query.EQ("MessageType", MessageType.Received)
                                  )
                              };

            if(!(folders.Length == 1 && folders[0].Equals("")))
            {
                if(folders[0] == FilterKeys.Message.STARRED)
                {
                    queries.Add(Query.EQ("IsStarred", true));
                }
                else if(folders[0] == FilterKeys.Message.UNREAD)
                {
                    queries.Add(Query.EQ("IsRead", false));
                }
            }

            var cursor = messages.Find(Query.And(queries.ToArray()));
            return cursor.SetSortOrder(SortBy.Descending("Created"));
        }

        public MongoCursor<Message> SearchOut(string[] folders, string filter)
        {
            var messages = _db.GetCollection<Message>(Collections.Messages);

            var queries = new List<IMongoQuery>()
                              {
                                  Query.And
                                  (
                                    Query.EQ("Owner", _identity.UserId),
                                    Query.EQ("MessageType", MessageType.Sent)
                                  )
                              };

            var cursor = messages.Find(Query.And(queries.ToArray()));
            return cursor.SetSortOrder(SortBy.Descending("Created")); ;
        }

        public int UnreadMessageCount()
        {
            return _db.GetCollection<Message>(Collections.Messages)
                .AsQueryable()
                .Count(x => x.Owner == _identity.UserId && x.MessageType == MessageType.Received && !x.IsRead);
        }
    }
}