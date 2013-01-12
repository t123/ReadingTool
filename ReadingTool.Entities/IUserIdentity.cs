using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace ReadingTool.Entities
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
            return (uIdentity.Roles ?? new string[] { }).Contains(role);
        }
    }

    public interface IUserIdentity : IIdentity
    {
        ObjectId UserId { get; }
        bool IsInRole(string role);
    }

    public class UserIdentity : IUserIdentity
    {
        #region interface members
        public bool IsAuthenticated { get; private set; }
        public string AuthenticationType { get; private set; }
        public string Name { get; set; }
        public string[] Roles { get; set; }

        public bool IsInRole(string role)
        {
            return (this.Roles ?? new string[] { }).Contains(role);
        }
        #endregion

        public ObjectId UserId { get; private set; }
        public string EmailAddress { get; set; }
        public string Theme { get; set; }

        public UserIdentity(
            string userId,
            bool isAuthenticated,
            string name,
            string[] roles,
            string emailAddress,
            string theme,
            string authenticationType) :
            this(ObjectId.Parse(userId), isAuthenticated, name, roles, emailAddress, theme, authenticationType)
        {
        }

        public UserIdentity(
            ObjectId userId,
            bool isAuthenticated,
            string name,
            string[] roles,
            string emailAddress,
            string theme,
            string authenticationType)
        {
            UserId = userId;
            IsAuthenticated = isAuthenticated;
            Name = name;
            Roles = roles;
            EmailAddress = emailAddress;
            Theme = theme;
            AuthenticationType = authenticationType;
        }
    }
}
