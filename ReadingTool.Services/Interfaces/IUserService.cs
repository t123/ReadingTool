using System;
using ReadingTool.Entities;
using ReadingTool.Repository;

namespace ReadingTool.Services
{
    public interface IUserService
    {
        Repository<User> Repository { get; }
        User CreateUser(string username, string password);
        User ValidateUser(string username, string password);
        User ValidateUser(Guid userId, string password);
        bool UsernameExists(string username);
        bool UpdatePassword(User user, string password);
    }
}