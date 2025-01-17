namespace ChatApp.Controllers;

public interface IChatService
{
    List<ChatMessage> GetChatsForUser(string userId);
}

public class ChatService : IChatService
{
    public List<ChatMessage> GetChatsForUser(string userId)
    {
        return new List<ChatMessage> { new() { MessageBody = "Static Message", ReceiverId = "0", SenderId = "1" } };
    }
}