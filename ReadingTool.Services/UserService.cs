using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
