using ChatApp.Controllers;

namespace ChatApp;

public interface IUserService
{
    public Action<List<User>> OnUsersUpdated { get; set; }
    int? AddUser(string username, string avatar);
    void RemoveUser(int userId);
    List<User> GetActiveUsers();
    User? GetUser(int userId);
}