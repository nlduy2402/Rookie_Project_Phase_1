using Microsoft.EntityFrameworkCore;
using RetailSystem.Domain.Entities;

namespace RetailSystem.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // (optional) config thêm nếu cần
        }
    }
}
