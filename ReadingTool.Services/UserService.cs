using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReadingTool.Core;
using ReadingTool.Entities;
using ServiceStack.OrmLite;

namespace ReadingTool.Services
{
    public interface IUserService
    {
        void Create(string username, string password);
        void Save(User user, string newPassword = "");
        void Delete(User user);
        void Delete(long id);
        User Find(long id);
        User FindUserByUsername(string username);
        bool VerifyPassword(string attemptedPassword, string currentPassword);
    }

    public class UserService : IUserService
    {
        private readonly IDbConnection _db;

        public UserService(IDbConnection db)
        {
            _db = db;
        }

        public void Create(string username, string password)
        {
            var user = new User()
                {
                    Created = DateTime.Now,
                    DisplayName = username,
                    EmailAddress = string.Empty,
                    Modified = DateTime.Now,
                    Password = BCrypt.Net.BCrypt.HashString(password),
                    Roles = Constants.Roles.WEB,
                    Username = username
                };

            _db.Save(user);
        }

        public void Save(User user, string newPassword = "")
        {
            if(user == null)
            {
                return;
            }

            user.Username = (user.Username ?? "").Trim();
            user.EmailAddress = (user.EmailAddress ?? "").Trim();
            user.Modified = DateTime.Now;

            if(!string.IsNullOrEmpty(newPassword))
            {
                user.Password = BCrypt.Net.BCrypt.HashString(newPassword);
            }

            _db.Save(user);
        }

        public void Delete(User user)
        {
            throw new NotImplementedException();
        }

        public void Delete(long id)
        {
            throw new NotImplementedException();
        }

        public User Find(long id)
        {
            return id > 0 ? _db.GetById<User>(id) : null;
        }

        public User FindUserByUsername(string username)
        {
            return _db.QuerySingle<User>(new { Username = username });
        }

        public bool VerifyPassword(string attemptedPassword, string currentPassword)
        {
            return BCrypt.Net.BCrypt.Verify(attemptedPassword, currentPassword);
        }
    }
}
