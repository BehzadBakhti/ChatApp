using Microsoft.EntityFrameworkCore;

namespace ChatApp.Data;

public static class DataExtensions
{
    public static void MigrateDb(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ChatAppContext>();
        dbContext.Database.Migrate();
    }
}