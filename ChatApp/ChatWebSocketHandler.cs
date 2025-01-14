using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatApp;

public class ChatWebSocketHandler
{
    private readonly List<WebSocket> _activeSockets = new();
    private readonly Dictionary<string, WebSocket> _userSockets = new();
    private readonly IUserService _userService;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public ChatWebSocketHandler(IUserService userService)
    {
        _userService = userService;
        _userService.OnUsersUpdated = async (updatedUsers) =>
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

            Console.WriteLine("Message:" + message);
            var socketMessage = JsonSerializer.Deserialize<SocketMessage>(message);
            Console.WriteLine("MessageType:" + socketMessage?.Action);
            switch (socketMessage?.Action)
            {
                case "Message":
                    var chatMessage = JsonSerializer.Deserialize<ChatMessage>(socketMessage.Payload);
                    if (chatMessage != null)
                        await SendMessagesAsync(chatMessage);
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
        _userService.RemoveUser(int.Parse(userId));
    }

    private async Task BroadcastMessageAsync(string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);

        await _semaphore.WaitAsync();
        try
        {
            foreach (var socket in _activeSockets)
            {
                if (socket.State == WebSocketState.Open)
                {
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
        }
        finally
        {
            _semaphore.Release();
        }
    }


private async Task SendMessagesAsync(ChatMessage chatMessage)
    {
        if (_userSockets.TryGetValue(chatMessage.ReceiverId, out var receiverSocket))
        {
            var socketMessage = new
            {
                type = "MESSAGE",
                data = new
                {
                    User = _userService.GetUser(int.Parse(chatMessage.SenderId)),
                    Message = chatMessage.MessageBody
                }
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
    }
}

public class ChatMessage
{
    public required string SenderId { get; set; }
    public required string ReceiverId { get; set; }
    public required string? MessageBody { get; set; }
    //public DateTime TimeStamp { get; set; }
}

public class SocketMessage
{
    public string Action { get; set; }
    public string Payload { get; set; }
}




