#region License
// UserIdentity.cs is part of ReadingTool.Common
// 
// ReadingTool.Common is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Common is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Common. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2013 Travis Watt
#endregion

using System;
using System.Linq;
using System.Security.Principal;

namespace ReadingTool.Common
{
    public class UserPrincipal : IPrincipal
    {
        public bool IsInRole(string role)
        {
            UserIdentity uIdentity = (UserIdentity)Identity;
            return uIdentity.Data.Roles.Contains(role);
        }

        public UserPrincipal(UserIdentity userIdentity)
        {
            Identity = userIdentity;
        }

        public IIdentity Identity { get; private set; }
    }

    public class UserIdentity : IIdentity
    {
        public class UserData
        {
            public string EmailAddress { get; set; }
            public string Username { get; set; }
            public string DisplayName { get; set; }
            public string[] Roles { get; set; }
        }

        public string Name { get; private set; }
        public string AuthenticationType { get; private set; }
        public bool IsAuthenticated { get; private set; }
        public UserData Data { get; private set; }
        public Guid UserId { get { return Guid.Parse(Name); } }

        public UserIdentity(
            string userId,
            bool isAuthenticated,
            string authenticationType,
            UserData data
            ) :
            this(Guid.Parse(userId), isAuthenticated, authenticationType, data)
        {
        }

        public UserIdentity(
            Guid userId,
            bool isAuthenticated,
            string authenticationType,
            UserData data
            )
        {
            IsAuthenticated = isAuthenticated;
            AuthenticationType = authenticationType;
            Data = data;
            Name = userId.ToString();
        }
    }
}
