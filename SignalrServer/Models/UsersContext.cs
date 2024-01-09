using Microsoft.EntityFrameworkCore;

namespace SignalrServer.Models
{
    public class UsersContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public UsersContext(DbContextOptions options)
            : base(options)
        {
            
        }
    }
}
