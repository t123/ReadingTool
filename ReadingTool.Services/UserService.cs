using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using ReadingTool.Core;
using ReadingTool.Core.Database;
using ReadingTool.Entities;
using ReadingTool.Repository;

namespace ReadingTool.Services
{
    public interface IUserService : IRepository<User>
    {
        void Create(string username, string password);
        void Save(User user, string newPassword = "");
        User FindUserByUsername(string username);
        bool UserExists(string username);
        bool VerifyPassword(string attemptedPassword, string currentPassword);
        User FindUserByApiKey(string apiKeyHeaderValue);
    }

    public class UserService : Repository<User>, IUserService
    {
        private readonly IDeleteService _deleteService;

        public UserService(MongoContext db, IDeleteService deleteService)
            : base(db)
        {
            _deleteService = deleteService;
        }

        public void Create(string username, string password)
        {
            var user = new User()
                {
                    DisplayName = (username ?? "").Trim(),
                    EmailAddress = string.Empty,
                    Modified = DateTime.Now,
                    Password = BCrypt.Net.BCrypt.HashString(password),
                    Roles = new[] { Constants.Roles.WEB },
                    Username = (username ?? "").Trim()
                };

            base.Save(user);
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

            base.Save(user);
        }

        public new void Delete(User user)
        {
            _deleteService.DeleteUser(user);
        }

        public new void Delete(ObjectId id)
        {
            Delete(FindOne(id));
        }

        public User FindUserByUsername(string username)
        {
            username = (username ?? "").Trim();
            return _collection.AsQueryable().FirstOrDefault(x => x.Username == username);
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

        public User FindUserByApiKey(string apiKey)
        {
            if(string.IsNullOrWhiteSpace(apiKey))
            {
                return null;
            }

            var user = _collection.AsQueryable().FirstOrDefault(x => x.Api.ApiKey == apiKey);

            if(user == null || !user.Api.IsEnabled)
            {
                return null;
            }

            //TODO any other logic here

            return user;
        }
    }
}
