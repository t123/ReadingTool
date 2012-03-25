#region License
// User.cs is part of ReadingTool.Entities
// 
// ReadingTool.Entities is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Entities is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Entities. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ReadingTool.Common.Enums;
using ReadingTool.Common.Helpers;

namespace ReadingTool.Entities
{
    public class User
    {
        public const string CollectionName = @"Users";

        public static class AllowedRoles
        {
            public const string ADMIN = @"admin";
            public const string DEVELOPER = @"developer";
        }

        [BsonId]
        public ObjectId UserId { get; set; }

        private string _username;

        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                UsernameLower = value.ToLowerInvariant();
            }
        }

        public string UsernameLower { get; private set; }
        public string DisplayName { get; set; }

        private string _emailAddress;

        public string EmailAddress
        {
            get { return _emailAddress; }
            set
            {
                _emailAddress = (value ?? "").ToLowerInvariant().Trim();
                EmailAddressMD5 = string.IsNullOrEmpty(_emailAddress) ? "" : PasswordHelper.CalculateMD5(_emailAddress);
            }
        }

        public string EmailAddressMD5 { get; private set; }
        public string[] Roles { get; set; }
        public string Password { get; set; }
        public bool ReceiveMessages { get; set; }
        public bool ShareWords { get; set; }

        public ObjectId? NativeLanguageId { get; set; }

        [BsonIgnore]
        public string UnencryptedPassword { get; set; }

        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public DateTime LastActivity { get; set; }

        public UserState State { get; set; }

        [BsonIgnoreIfNull]
        public MediaControl MediaControl { get; set; }
        [BsonIgnoreIfNull]
        public Style Style { get; set; }
        public PublicProfile PublicProfile { get; set; }

        public DateTime? LastLogin { get; set; }

        [BsonIgnore]
        public string Fullname
        {
            get
            {
                if(string.IsNullOrEmpty(DisplayName)) return Username;
                return Username + " (" + DisplayName + ")";
            }
        }

        public User()
        {
            PublicProfile = new PublicProfile();
        }
    }
}
