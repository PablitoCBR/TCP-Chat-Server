using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class ChattyDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public ChattyDbContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite("Data Source=Chatty.db");
    }
}
