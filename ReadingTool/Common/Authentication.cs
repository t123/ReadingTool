#region License
// Authentication.cs is part of ReadingTool
// 
// ReadingTool is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using AutoMapper;
using MongoDB.Bson;
using Newtonsoft.Json;
using ReadingTool.Common.Helpers;
using ReadingTool.Entities;
using ReadingTool.Entities.Identity;
using ReadingTool.Services;

namespace ReadingTool.Common
{
    public class SecurityManager
    {
        private readonly IUserService _userService;

        public SecurityManager(IUserService userService)
        {
            _userService = userService;
        }

        public static string ConstructUserData(User user)
        {
            var cookieModel = Mapper.Map<User, UserCookieModel>(user);
            var json = JsonConvert.SerializeObject(cookieModel, Formatting.None);
            return json;
        }

        public User Authenticate(string username, string password)
        {
            User user = _userService.FindOneByUsername(username);

            if(user == null)
            {
                return null;
            }

            var correctPassword = PasswordHelper.Verify(password, user.Password);

            if(correctPassword)
            {
                _userService.UpdateLastLogin(user.UserId);
                return user;
            }

            return null;
        }

        public static UserPrincipal ConstructUserPrincipal(IIdentity identity, UserCookieModel data)
        {
            ObjectId userId = GetUserIdFromIdentity(identity);

            try
            {
                UserIdentity newIdentity = new UserIdentity(
                    userId,
                    identity.IsAuthenticated,
                    identity.AuthenticationType,
                    data
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

        protected static ObjectId GetUserIdFromIdentity(IIdentity identity)
        {
            return ObjectId.Parse(identity.Name);
        }

        public static HttpCookie CreateAuthenticationTicket(User user)
        {
            if(user == null)
                return null;

            string userData = ConstructUserData(user);

            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                1,
                user.UserId.ToString(),
                DateTime.Now,
                DateTime.Now.AddMinutes(1 * 60 * 24),
                true,
                userData);

            string encTicket = FormsAuthentication.Encrypt(ticket);
            return new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
        }
    }
}
