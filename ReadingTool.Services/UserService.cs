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
        void Delete(Guid id);
        User Find(Guid id);
        User FindUserByUsername(string username);
        bool UserExists(string username);
        bool VerifyPassword(string attemptedPassword, string currentPassword);
    }

    public class UserService : IUserService
    {
        private readonly IDbConnection _db;
        private readonly IDeleteService _deleteService;

        public UserService(IDbConnection db, IDeleteService deleteService)
        {
            _db = db;
            _deleteService = deleteService;
        }

        public void Create(string username, string password)
        {
            var user = new User()
                {
                    Id = SequentialGuid.NewGuid(),
                    Created = DateTime.Now,
                    DisplayName = (username ?? "").Trim(),
                    EmailAddress = string.Empty,
                    Modified = DateTime.Now,
                    Password = BCrypt.Net.BCrypt.HashString(password),
                    Roles = Constants.Roles.WEB,
                    Username = (username ?? "").Trim()
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
            _deleteService.DeleteUser(user);
        }

        public void Delete(Guid id)
        {
            Delete(Find(id));
        }

        public User Find(Guid id)
        {
            return id == Guid.Empty ? null : _db.GetById<User>(id);
        }

        public User FindUserByUsername(string username)
        {
            username = (username ?? "").Trim();
            return _db.QuerySingle<User>(new { Username = username });
        }

        public bool UserExists(string username)
        {
            return FindUserByUsername(username) != null;
        }

        public bool VerifyPassword(string attemptedPassword, string currentPassword)
        {
#if DEBUG
            return true;
#else
            return BCrypt.Net.BCrypt.Verify(attemptedPassword, currentPassword);
#endif
        }
    }
}
