using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RetailSystem.Domain.Entities;
using System.Text.Json;

namespace RetailSystem.Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<AdminAccount> AdminAccounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)    
                      .HasForeignKey(p => p.CategoryId) 
                      .OnDelete(DeleteBehavior.Restrict); 
            });


            modelBuilder.Entity<ProductImage>()
                .HasOne(i => i.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<UserToken>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AdminAccount>().HasData(
                new AdminAccount
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = "123",
                    Role = "Admin"
                }
            );
        }
    }
}
