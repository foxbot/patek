using Microsoft.EntityFrameworkCore;

namespace Patek.Data
{
    public class PatekContext : DbContext
    {
        public PatekContext(DbContextOptions options) : base(options) { }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Audit> Events { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tag>()
                .HasAlternateKey(t => t.Name);
        }
    }
}
