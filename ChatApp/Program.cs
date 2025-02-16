
using System.Net.WebSockets;
using System.Text;
using ChatApp.Controllers;
using ChatApp.Data;

namespace ChatApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connString = builder.Configuration.GetConnectionString("ChatApp");
            builder.Services.AddSqlite<ChatAppContext>(connString);
            // Add services to the container.
            builder.Services.AddControllersWithViews();
           // var userService = new UserService();

            builder.Services.AddSingleton<IUserService, UserService>();
            builder.Services.AddSingleton<IChatService, ChatService>();
            builder.Services.AddSingleton<ChatWebSocketHandler>();

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
            app.MigrateDb();

            app.Run();
        }
    }
}
