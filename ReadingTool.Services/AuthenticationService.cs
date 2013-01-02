using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using Newtonsoft.Json;
using ReadingTool.Core;
using ReadingTool.Entities;

namespace ReadingTool.Services
{
    public interface IAuthenticationService
    {
        User Authenticate(string username, string password);
        HttpCookie CreateAuthenticationTicket(User user);
        UserPrincipal ConstructUserPrincipal(IIdentity identity, string authTicket);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserService _userService;

        internal class UserTicketData
        {
            public string DisplayName { get; set; }
            public string Roles { get; set; }
            public string Theme { get; set; }
        }

        public AuthenticationService(IUserService userService)
        {
            _userService = userService;
        }

        private string ConstructUserData(User user)
        {
            UserTicketData data = new UserTicketData()
                {
                    DisplayName = user.DisplayName,
                    Theme = string.IsNullOrWhiteSpace(user.Theme) ? "default" : user.Theme,
                    Roles = user.Roles
                };

            return JsonConvert.SerializeObject(data);
        }

        public User Authenticate(string username, string password)
        {
            User user = _userService.FindUserByUsername(username);

            if(user == null)
                return null;

#if DEBUG
            return user;
#else
            if(user.Roles.Contains(Constants.Roles.WEB) && _userService.VerifyPassword(password, user.Password))
            {
                return user;
            }

            return null;
#endif
        }

        public UserPrincipal ConstructUserPrincipal(IIdentity identity, string authTicket)
        {
            try
            {
                var userId = GetUserIdFromIdentity(identity);
                UserTicketData data = JsonConvert.DeserializeObject<UserTicketData>(authTicket);

                UserIdentity newIdentity = new UserIdentity(
                   userId,
                   identity.IsAuthenticated,
                   data.DisplayName,
                   data.Roles,
                   "",
                   data.Theme,
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

        protected Guid GetUserIdFromIdentity(IIdentity identity)
        {
            return Guid.Parse(identity.Name);
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
