using ChatApp.Controllers;

namespace ChatApp;

public class UserService : IUserService
{
    private readonly List<User> _activeUsers = new();// temp database for users

    public Action<List<User>> OnUsersUpdated { get; set; }

    public int? AddUser(string username, string avatar)
    {
        if (_activeUsers.Find(user => user.Username == username) != null) // implement equality comparer
            return null;// Error User name is not available

        var id = _activeUsers.Count;
        _activeUsers.Add(new User(id, username, avatar));
        OnUsersUpdated.Invoke(_activeUsers);
        return id;
    }

    public void RemoveUser(int userId)
    {
        _activeUsers.RemoveAt(userId);// Problematic
        OnUsersUpdated.Invoke(_activeUsers);
    }

    public List<User> GetActiveUsers()
    {
        return _activeUsers;
    }

    public User? GetUser(int userId)
    {
        return _activeUsers.Find(u => u.Id == userId);
    }

    public void AddChatRoom()
    {

    }
}

public record User(int Id, string Username, string Avatar);