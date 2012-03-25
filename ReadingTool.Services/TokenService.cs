#region License
// TokenService.cs is part of ReadingTool.Services
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
using System.Linq;
using FluentMongo.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using ReadingTool.Common.Helpers;
using ReadingTool.Entities;

namespace ReadingTool.Services
{
    public interface ITokenService
    {
        Token Save(Token token);
        Token Find(string tokenId);
    }

    public class TokenService : ITokenService
    {
        private readonly MongoDatabase _db;
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger("Services");

        public TokenService(MongoDatabase db)
        {
            _db = db;
        }

        private bool ApiAccess(ObjectId userId)
        {
#if DEBUG
            return true;
#endif
            var query = _db.GetCollection<ApiRequest>(Collections.APIRequests).AsQueryable();
            if(query.Count(x => x.UserId == userId && x.DateTime > DateTime.Now.AddHours(-1)) > ApiRequest.PER_HOUR)
            {
                return false;
            }

            if(query.Count(x => x.UserId == userId && x.DateTime > DateTime.Now.AddHours(-24)) > ApiRequest.PER_DAY)
            {
                return false;
            }

            return true;
        }

        public Token Save(Token token)
        {
            if(token.UserId == ObjectId.Empty)
            {
                throw new NotSupportedException("User cannot be an empty Id");
            }

            if(string.IsNullOrEmpty(token.TokenId))
            {
                string tokenId;

#if DEBUG
                tokenId = "ABC";
#else
                do
                {
                    tokenId = PasswordHelper.CreateRandomString(20, PasswordHelper.AllowedCharacters.AlphaNumericSpecial);
                } while(_db.GetCollection<Token>(Collections.Tokens).AsQueryable().Count(x => x.TokenId == tokenId) != 0);
#endif

                token.TokenId = tokenId;
            }

            token.Expiry = DateTime.Now.AddHours(2);
            _db.GetCollection(Collections.Tokens).Save(token);

            return token;
        }

        public Token Find(string tokenId)
        {
            var result = _db.GetCollection<Token>(Collections.Tokens)
                 .FindAndModify(
                     Query.And(Query.EQ("_id", tokenId), Query.GTE("Expiry", DateTime.Now)),
                     SortBy.Ascending("TokenId"),
                     Update.Set("Expiry", DateTime.Now.AddHours(2)),
                     true
                 );

            var token = result.GetModifiedDocumentAs<Token>();

            if(token != null)
            {
                _db.GetCollection<ApiRequest>(Collections.APIRequests)
                    .Save(new ApiRequest() { DateTime = DateTime.Now, UserId = token.UserId });

                if(!ApiAccess(token.UserId))
                {
                    token.IsValid = false;
                }
            }

            return token;
        }
    }
}
