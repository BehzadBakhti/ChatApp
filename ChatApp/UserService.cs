using ChatApp.Controllers;

namespace ChatApp;

public class UserService : IUserService
{
    private readonly Dictionary<string, User> _activeUsers = new();// temp database for users

    public event Action<List<User>>? OnUsersUpdated;

    public string? AddUser(string username, string avatar)
    {
        if (_activeUsers.TryGetValue(username, out _)) //implement equality comparer
            return null;// Error User name is not available

        var id = _activeUsers.Count.ToString();
        _activeUsers.Add(id, new User(id, username, avatar));
        OnUsersUpdated?.Invoke(_activeUsers.Values.ToList());
        return id;
    }

    public void RemoveUser(string userId)
    {
        _activeUsers.Remove(userId);// Problematic
        OnUsersUpdated?.Invoke(_activeUsers.Values.ToList());
    }

    public List<User> GetActiveUsers()
    {
        return _activeUsers.Values.ToList();
    }

    public User? GetUser(string userId)
    {
        return _activeUsers.TryGetValue(userId, out var user) ? user : null;
    }

    public void AddChatRoom()
    {

    }
}

public record User(string Id, string Username, string Avatar);