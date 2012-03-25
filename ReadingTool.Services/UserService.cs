#region License
// UserService.cs is part of ReadingTool.Services
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
using System.Text.RegularExpressions;
using FluentMongo.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using ReadingTool.Common.Enums;
using ReadingTool.Common.Helpers;
using ReadingTool.Entities;
using StructureMap;

namespace ReadingTool.Services
{
    public interface IUserService
    {
        void Save(User user);
        User FindOneByUsername(string username);
        User FindOne(string id);
        User FindOne(ObjectId id);
        void DeleteData(ObjectId userId, bool b);
        IEnumerable<User> AutocompleteUsername(string term);
        IEnumerable<User> FindAllById(IEnumerable<ObjectId> userId);
        void UpdateLastLogin(ObjectId userId);
        void GenerateForgottenPassword(User user, string encrypted);
        string ConfirmPasswordReset(string username, string resetKey);
        void ResetPassword(string username, string password);
        long UserCount();
        IEnumerable<User> FindAll();
        Tuple<long, IEnumerable<User>> FindAll(int page);
    }

    public class UserService : IUserService
    {
        private readonly MongoDatabase _db;

        public UserService(MongoDatabase db)
        {
            _db = db;
        }

        public void Save(User user)
        {
            if(user.UserId == ObjectId.Empty)
            {
                if(_db.GetCollection(Collections.Users).Count() == 0)
                {
                    user.Roles = new[] { User.AllowedRoles.ADMIN, User.AllowedRoles.DEVELOPER };
                }
                else
                {
                    user.Roles = new[] { "" };
                }

                user.Created = DateTime.Now;
                user.State = UserState.Active;
                string password = user.UnencryptedPassword;

                if(string.IsNullOrEmpty(password))
                {
                    password = PasswordHelper.RandomPassword();
                }

                user.Password = PasswordHelper.HashString(password);
            }
            else
            {
                if(!string.IsNullOrEmpty(user.UnencryptedPassword))
                {
                    user.Password = PasswordHelper.HashString(user.UnencryptedPassword);
                }
            }

            user.Modified = DateTime.Now;

            _db.GetCollection(Collections.Users).Save(user);
        }

        public User FindOne(ObjectId id)
        {
            return _db.GetCollection<User>(Collections.Users).FindOneById(id);
        }

        public void DeleteData(ObjectId userId, bool deleteAccount)
        {
            _db.GetCollection(Collections.Words).Remove(Query.EQ("Owner", userId));
            _db.GetCollection(Collections.Items).Remove(Query.EQ("Owner", userId));
            _db.GetCollection(Collections.Languages).Remove(Query.EQ("Owner", userId));

            if(deleteAccount)
            {
                _db.GetCollection(Collections.Users).Remove(Query.EQ("_id", userId));
            }
        }

        public IEnumerable<User> AutocompleteUsername(string name)
        {
            var lowered = (name ?? "").ToLowerInvariant();
            return
                _db.GetCollection<User>(Collections.Users)
                    .Find(
                        Query.And(
                            Query.EQ("ReceiveMessages", true),
                            Query.Matches("UsernameLower", BsonRegularExpression.Create(new Regex("^" + lowered)))
                            )
                    )
                    .SetSortOrder(SortBy.Ascending("UsernameLower"))
                    .SetLimit(15);
        }

        public IEnumerable<User> FindAllById(IEnumerable<ObjectId> userId)
        {
            return _db.GetCollection<User>(Collections.Users)
                .AsQueryable()
                .Where(x => userId.Contains(x.UserId));
        }

        public void UpdateLastLogin(ObjectId userId)
        {
            _db.GetCollection(Collections.Users)
                .Update
                (
                    Query.EQ("_id", userId),
                    Update.Set("LastLogin", DateTime.Now)
                );
        }

        public void GenerateForgottenPassword(User user, string encrypted)
        {
            var reset = new PasswordReset()
                            {
                                Created = DateTime.Now,
                                ResetKey = encrypted,
                                UserId = user.UserId,
                                Username = user.Username
                            };

            _db.GetCollection(Collections.PasswordResets).Save(reset);

            var emailService = ObjectFactory.GetInstance<IEmailService>();
            emailService.ForgotPassword(user, reset);
        }

        public string ConfirmPasswordReset(string username, string resetKey)
        {
            username = (username ?? "").Trim().ToLowerInvariant();

            PasswordReset reset = _db.GetCollection<PasswordReset>(Collections.PasswordResets)
                .AsQueryable()
                .FirstOrDefault(x => x.Username == username && x.ResetKey == resetKey);

            if(reset == null)
            {
                return "Either you username or reset key is incorrect";
            }

            if((DateTime.Now - reset.Created).TotalHours > 48)
            {
                return "Your reset key has expired. Please request a new key.";
            }

            return string.Empty;
        }

        public void ResetPassword(string username, string password)
        {
            username = (username ?? "").Trim().ToLowerInvariant();
            var encryped = PasswordHelper.HashString(password);

            _db.GetCollection<User>(Collections.Users)
                .Update
                (
                    Query.EQ("Username", username),
                    Update.Set("Password", encryped)
                );

            _db.GetCollection(Collections.PasswordResets).Remove(Query.EQ("Username", username));

            var user = FindOneByUsername(username);

            if(user != null)
            {
                var emailService = ObjectFactory.GetInstance<IEmailService>();
                emailService.ResetPasswordConfirmation(user);
            }
        }

        public long UserCount()
        {
            return _db.GetCollection(Collections.Users).Count();
        }

        public IEnumerable<User> FindAll()
        {
            return _db.GetCollection<User>(Collections.Users)
                .FindAll()
                .SetSortOrder(SortBy.Ascending("UsernameLower"));
        }

        public Tuple<long, IEnumerable<User>> FindAll(int page)
        {
            var cursor = _db.GetCollection<User>(Collections.Users)
                .FindAll()
                .SetSortOrder(SortBy.Ascending("UsernameLower"))
                .SetSkip((page - 1) * 20).SetLimit(20)
                ;

            return new Tuple<long, IEnumerable<User>>(
                cursor.Count(),
                cursor
                );
        }

        public User FindOneByUsername(string username)
        {
            string user = (username ?? "").ToLowerInvariant();
            return _db.GetCollection<User>(Collections.Users)
                .AsQueryable()
                .FirstOrDefault(x => x.UsernameLower == user);
        }

        public User FindOne(string id)
        {
            return FindOne(new ObjectId(id));
        }
    }
}