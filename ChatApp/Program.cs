
using System.Net.WebSockets;
using System.Text;

namespace ChatApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            
            var app = builder.Build();
            app.UseStaticFiles();

            app.UseWebSockets();
            app.Map("/ws", async context =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    await EchoMessagesAsync(webSocket);
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            });



            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }

        static async Task EchoMessagesAsync(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];

            try
            {

                WebSocketReceiveResult result;

                do
                {
                    // Receive a message
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        // Gracefully close the WebSocket when a Close message is received
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", CancellationToken.None);
                        break;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var response = $"Server: {message}";
                    await webSocket.SendAsync(Encoding.UTF8.GetBytes(response), result.MessageType, result.EndOfMessage,
                        CancellationToken.None);
                    await Task.Delay(1000);
                    await webSocket.SendAsync("Hi There!"u8.ToArray(), result.MessageType, result.EndOfMessage,
                        CancellationToken.None);
                } while (!result.CloseStatus.HasValue);
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"WebSocket error: {ex.Message}");
                // Handle WebSocket-specific errors (e.g., aborted connections)
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error: {ex.Message}");
                // Handle unexpected errors
            }
            finally
            {
                if (webSocket.State == WebSocketState.Open)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Unexpected server error",
                        CancellationToken.None);
                }

                webSocket.Dispose();
            }
        }
    }
}
