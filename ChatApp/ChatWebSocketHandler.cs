using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChatApp.Controllers;

namespace ChatApp;

public class ChatWebSocketHandler
{
    private readonly List<WebSocket> _activeSockets = new();
    private readonly Dictionary<string, WebSocket> _userSockets = new();
    private readonly IUserService _userService;
    private readonly IChatService _chatService;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public ChatWebSocketHandler(IUserService userService, IChatService chatService)
    {
        _userService = userService;
        _chatService = chatService;
        _userService.OnUsersUpdated += async (updatedUsers) =>
        {
            var updateMessage = JsonSerializer.Serialize(new { type = "USER_LIST", data = updatedUsers });
            await BroadcastMessageAsync(updateMessage);
        };
    }

    public async Task HandleWebSocketAsync(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = 400;
            return;
        }

        var userId = context.Request.Query["userId"].ToString();

        if (string.IsNullOrEmpty(userId))
        {
            context.Response.StatusCode = 400;
            return;
        }

        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        _userSockets[userId] = webSocket;
        _activeSockets.Add(webSocket);

        var buffer = new byte[1024 * 4];
        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!result.CloseStatus.HasValue)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var socketMessage = JsonSerializer.Deserialize<SocketMessage>(message);

            switch (socketMessage?.Action)
            {
                case "Message":
                    var chatMessage = JsonSerializer.Deserialize<ChatMessage>(socketMessage.Payload);
                    if (chatMessage != null)
                    {
                        await DeliverMessagesAsync(chatMessage);
                    }
                    break;
                case "NewChatRoom":
                    break;
                default:
                    throw new InvalidOperationException("Unknown message type");
            }

            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        _userSockets.Remove(userId);
        _activeSockets.Remove(webSocket);
        _userService.RemoveUser(userId);
    }

    private async Task BroadcastMessageAsync(string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);

        await _semaphore.WaitAsync();
        try
        {
            foreach (var socket in _activeSockets)
            {
                if (socket.State != WebSocketState.Open) 
                    continue;
                
                try
                {
                    await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                }
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task DeliverMessagesAsync(ChatMessage chatMessage)
    {
        if (_userSockets.TryGetValue(chatMessage.ReceiverId, out var receiverSocket))
        {
            var socketMessage = new
            {
                type = "MESSAGE",
                data = chatMessage
            };

            try
            {
                await receiverSocket.SendAsync(
                    Encoding.UTF8.GetBytes(JsonSerializer.Serialize(socketMessage)),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
            }
        }
        _chatService.AddChatMessage(chatMessage);
    }
}