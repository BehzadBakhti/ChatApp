using ChatApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Data
{
    public class ChatAppContext(DbContextOptions<ChatAppContext> options)
        : DbContext(options)
    {
        public DbSet<ChatMessageEntity> ChatMessages => Set<ChatMessageEntity>();
        public DbSet<UserEntity> Users=> Set<UserEntity>();
    }
}
