using Microsoft.EntityFrameworkCore;

namespace security.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } 
        public DbSet<Reclamation> Reclamations { get; set;}
    }
}
