using ChatApp.Controllers;

namespace ChatApp;

public interface IUserService
{
    event Action<List<User>> OnUsersUpdated;
    string? AddUser(string username, string avatar);
    void RemoveUser(string userId);
    List<User> GetActiveUsers();
    User? GetUser(string userId);
}