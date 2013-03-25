using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

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
        public long UserId { get { return long.Parse(Name); } }

        public UserIdentity(
            string userId,
            bool isAuthenticated,
            string authenticationType,
            UserData data
            ) :
            this(long.Parse(userId), isAuthenticated, authenticationType, data)
        {
        }

        public UserIdentity(
            long userId,
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
