
using System.Net.WebSockets;
using System.Text;
using ChatApp.Controllers;

namespace ChatApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            var userService = new UserService();

            builder.Services.AddSingleton<IUserService>(userService);
            var chatHandler = new ChatWebSocketHandler(userService);
            builder.Services.AddSingleton(chatHandler);
            builder.Services.AddSingleton<IChatService, ChatService>();

            var app = builder.Build();
            app.UseStaticFiles();

            app.UseWebSockets();
            app.Map("/ws", async context =>
            {
                var ch = context.RequestServices.GetRequiredService<ChatWebSocketHandler>();
                await ch.HandleWebSocketAsync(context);
            });

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
