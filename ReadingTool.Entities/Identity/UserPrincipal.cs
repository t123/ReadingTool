#region License
// UserPrincipal.cs is part of ReadingTool.Entities
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

using System.Linq;
using System.Security.Principal;

namespace ReadingTool.Entities.Identity
{
    public class UserPrincipal : IPrincipal
    {
        public IIdentity Identity { get; private set; }

        public UserPrincipal(IIdentity userIdentity)
        {
            Identity = userIdentity;
        }

        public bool IsInRole(string role)
        {
            UserIdentity uIdentity = (UserIdentity)Identity;

            if(uIdentity.Roles.Contains(role))
                return true;

            return false;
        }
    }
}