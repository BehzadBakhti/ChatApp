using System.Reflection.Metadata.Ecma335;

namespace ChatApp.Controllers;

public interface IChatService
{
    List<ChatMessage>? GetChatsForUser(string firstUserId, string secondUserId);
    void AddChatMessage(ChatMessage message);
}