namespace ChatApp.Controllers;

public class ChatService : IChatService
{
    private readonly Dictionary<(string, string), List<ChatMessage>> _chatMessages = new();
    public List<ChatMessage>? GetChatsForUser(string firstUserId, string secondUserId)
    {
        var userCombination = NormalizeKey(firstUserId, secondUserId);
        _chatMessages.TryGetValue(userCombination, out var result);
        return result;
    }

    public void AddChatMessage(ChatMessage message)
    {
        var userCombination = NormalizeKey(message.ReceiverId, message.SenderId);
        if (_chatMessages.TryGetValue(userCombination, out var messageList))
        {
            messageList.Add(message);
        }
        else
        {
            _chatMessages.Add(userCombination, new List<ChatMessage> { message });
        }
    }

    private static (string, string) NormalizeKey(string key1, string key2)
    {
        return int.Parse(key1) > int.Parse(key2) ? (key1, key2) : (key2, key1);
    }
}