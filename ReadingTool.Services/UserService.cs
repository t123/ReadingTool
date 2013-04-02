#region License
// UserService.cs is part of ReadingTool.Services
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
// Copyright (C) 2013 Travis Watt
#endregion

using System;
using System.IO;
using System.Linq;
using System.Security.Principal;
using ReadingTool.Common;
using ReadingTool.Entities;
using ReadingTool.Repository;

namespace ReadingTool.Services
{
    public class UserService : IUserService
    {
        private readonly Repository<User> _userRepository;
        private readonly IEmailService _emailService;
        private log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public Repository<User> Repository { get { return _userRepository; } }
        private readonly UserIdentity _identity;

        public UserService(Repository<User> userRepository)
            : this(userRepository, null, new EmailService())
        {

        }

        public UserService(Repository<User> userRepository, IPrincipal principal)
            : this(userRepository, principal, new EmailService())
        {

        }

        public UserService(Repository<User> userRepository, IPrincipal principal, IEmailService emailService)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _identity = principal.Identity as UserIdentity;
        }

        public User CreateUser(string username, string password)
        {
            try
            {
                var userCount = _userRepository.FindAll().Count();

                User user = new User()
                    {
                        Created = DateTime.Now,
                        Username = username.Trim(),
                        DisplayName = username.Trim(),
                        Password = BCrypt.Net.BCrypt.HashPassword(password),
                        ApiKey = CreateApiKey(),
                    };

                if(userCount == 0)
                {
                    user.Roles = new string[] { Roles.ADMIN };
                }

                _userRepository.Save(user);
                return user;
            }
            catch(Exception e)
            {
                _logger.Error(e);
                return null;
            }
        }

        public User ValidateUser(string username, string password)
        {
            var user = _userRepository.FindOne(x => x.Username == username);

            if(user == null)
            {
                return null;
            }

            if(BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                if(user.ForgotPasswordRequest != null)
                {
                    user.ForgotPasswordRequest.Clear();
                    _userRepository.Save(user);
                }

                return user;
            }

            return null;
        }

        public User ValidateUser(Guid userId, string password)
        {
            var user = _userRepository.FindOne(userId);

            if(user == null)
            {
                return null;
            }

            if(BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return user;
            }

            return null;
        }

        public bool UsernameExists(string username)
        {
            username = (username ?? "").Trim();
            return _userRepository.FindAll(x => x.Username == username).Any();
        }

        public bool EmailExists(string emailAddress)
        {
            emailAddress = (emailAddress ?? "").Trim();
            return _userRepository.FindAll(x => x.EmailAddress == emailAddress).Any();
        }

        public void CreateResetKey(string emailAddress)
        {
            emailAddress = (emailAddress ?? "").Trim().ToLowerInvariant();
            var user = _userRepository.FindOne(x => x.EmailAddress == emailAddress);

            if(user == null)
            {
                return;
            }

            user.ForgotPasswordRequest.Clear();
            user.ForgotPasswordRequest.Add(new ForgotPasswordRequest()
                {
                    Expires = DateTime.Now.AddMinutes(60),
                    ResetKey = System.Web.Security.Membership.GeneratePassword(50, 15),
                    User = user
                });

            _userRepository.Save(user);
            _emailService.ResetPasswordInstructions(user);
        }

        public bool ResetPassword(string username, string key, string password)
        {
            var user = _userRepository.FindOne(x => x.Username == username);

            if(user == null || user.ForgotPasswordRequest.First() == null)
            {
                return false;
            }

            if(user.ForgotPasswordRequest.First().Expires < DateTime.Now)
            {
                return false;
            }

            if(!key.Equals(user.ForgotPasswordRequest.First().ResetKey))
            {
                return false;
            }

            UpdatePassword(user, password);
            _emailService.ResetSuccess(user);

            return true;
        }

        public bool UpdatePassword(User user, string password)
        {
            if(user == null)
            {
                return false;
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(password);
            user.ForgotPasswordRequest.Clear();
            _userRepository.Save(user);

            return true;
        }

        public string CreateApiKey()
        {
            return System.Web.Security.Membership.GeneratePassword(50, 15);
        }

        public User GetUserByApiKey(string apiKey)
        {
            return _userRepository.FindOne(x => x.ApiKey == apiKey);
        }

        public void DeleteAccount()
        {
            if(_identity != null)
            {
                var user = _userRepository.FindOne(_identity.UserId);

                if(user == null)
                {
                    return;
                }

                _userRepository.Delete(user);

                string path = UserDirectory.GetDirectory(_identity.UserId);

                if(Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
        }
    }
}
