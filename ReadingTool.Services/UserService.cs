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
using System.Linq;
using ReadingTool.Common;
using ReadingTool.Entities;
using ReadingTool.Repository;

namespace ReadingTool.Services
{
    public class UserService : IUserService
    {
        private readonly Repository<User> _userRepository;
        private log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public Repository<User> Repository { get { return _userRepository; } }

        public UserService(Repository<User> userRepository)
        {
            _userRepository = userRepository;
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
                        Password = BCrypt.Net.BCrypt.HashPassword(password),
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
                return user;
            }

            return null;
        }

        public User ValidateUser(long userId, string password)
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
            return _userRepository.FindAll(x => x.Username == username).Any();
        }

        public bool UpdatePassword(User user, string password)
        {
            if(user == null)
            {
                return false;
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(password);
            _userRepository.Save(user);

            return true;
        }
    }
}
