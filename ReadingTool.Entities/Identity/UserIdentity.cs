#region License
// UserIdentity.cs is part of ReadingTool.Entities
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

using System.Security.Principal;
using MongoDB.Bson;

namespace ReadingTool.Entities.Identity
{
    public class UserForService
    {
        public ObjectId UserId { get; private set; }

        public UserForService()
        {
            UserId = ObjectId.Empty;
        }

        public UserForService(ObjectId userId)
        {
            UserId = userId;
        }

        public UserForService(IIdentity identity)
        {
            UserId = ObjectId.Empty;

            if(identity != null && identity is IUserIdentity)
            {
                UserId = (identity as IUserIdentity).UserId;
            }
        }
    }

    public interface IUserIdentity : IIdentity
    {
        ObjectId UserId { get; }
        string[] Roles { get; set; }
    }

    public class UserIdentity : IUserIdentity
    {
        #region interface members
        public bool IsAuthenticated { get; private set; }
        public string AuthenticationType { get; private set; }
        public string Name { get; set; }
        public string[] Roles { get; set; }
        #endregion

        public ObjectId UserId { get; private set; }
        public string DisplayName { get; set; }

        public UserIdentity(
            ObjectId userId,
            bool isAuthenticated,
            string authenticationType,
            UserCookieModel data
            )
        {
            UserId = userId;
            IsAuthenticated = isAuthenticated;
            AuthenticationType = authenticationType;

            Name = data.Username;
            Roles = data.Roles;
            DisplayName = data.DisplayName;
        }
    }
}