using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using ReadingTool.Entities;

namespace ReadingTool.Services
{
    public interface IAuthenticationService
    {
        User Authenticate(string username, string password);
        HttpCookie CreateAuthenticationTicket(User user);
        Dictionary<string, string> GetUserData(string userData);
        UserPrincipal ConstructUserPrincipal(IIdentity identity, string displayName, string roles);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserService _userService;
        private const int INDEX_USERID = 0;
        private const int INDEX_DISPLAYNAME = 1;
        private const int INDEX_ROLES = 2;
        public const string USER_ID = @"USER_ID";
        public const string DISPLAYNAME = @"DISPLAYNAME";
        public const string ROLES = @"ROLES";

        public AuthenticationService(IUserService userService)
        {
            _userService = userService;
        }

        private string ConstructUserData(User user)
        {
            string userData = string.Format(
                //UserID,DisplayName,Roles
                    @"{0};{1};{2}",
                    user.Id,
                    user.DisplayName,
                    user.Roles
                    );

            return userData;
        }

        public Dictionary<string, string> GetUserData(string userData)
        {
            var d = new Dictionary<string, string>();
            var split = userData.Split(';');

            d[USER_ID] = split[INDEX_USERID];
            d[DISPLAYNAME] = split[INDEX_DISPLAYNAME];
            d[ROLES] = split[INDEX_ROLES];

            return d;
        }

        public User Authenticate(string username, string password)
        {
            User user = _userService.FindUserByUsername(username);

            if(user == null)
                return null;

#if DEBUG
            return user;
#else
            if(_userService.VerifyPassword(password, user.Password))
            {
                return user;
            }
            return null;
#endif
        }

        public UserPrincipal ConstructUserPrincipal(IIdentity identity, string displayName, string roles)
        {
            var userId = GetUserIdFromIdentity(identity);

            try
            {
                UserIdentity newIdentity = new UserIdentity(
                    userId,
                    identity.IsAuthenticated,
                    displayName,
                    roles,
                    "",
                    identity.AuthenticationType
                    );

                IPrincipal userPrincipal = new UserPrincipal(newIdentity);

                return userPrincipal as UserPrincipal;
            }
            catch
            {
                FormsAuthentication.SignOut();
                throw new NullReferenceException("User principal was not created");
            }
        }

        protected long GetUserIdFromIdentity(IIdentity identity)
        {
            return long.Parse(identity.Name);
        }

        public HttpCookie CreateAuthenticationTicket(User user)
        {
            if(user == null)
                return null;

            string userData = ConstructUserData(user);

            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                1,
                user.Id.ToString(),
                DateTime.Now,
                DateTime.Now.AddMinutes(1 * 60 * 24),
                true,
                userData);

            string encTicket = FormsAuthentication.Encrypt(ticket);
            return new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
        }
    }
}
