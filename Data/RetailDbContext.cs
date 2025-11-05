using Microsoft.EntityFrameworkCore;
using RetailDemo.Models;

namespace RetailDemo.Data
{
    public class RetailDbContext : DbContext
    {
        public RetailDbContext(DbContextOptions<RetailDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Recommendation> Recommendations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed Products
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = Guid.NewGuid(), Name = "Laptop", Category = "Electronics", Price = 1200 },
                new Product { Id = Guid.NewGuid(), Name = "Headphones", Category = "Electronics", Price = 150 },
                new Product { Id = Guid.NewGuid(), Name = "Coffee Maker", Category = "Home Appliances", Price = 80 }
            );

            // Seed Inventory
            modelBuilder.Entity<InventoryItem>().HasData(
                new InventoryItem { Id = Guid.NewGuid(), ProductName = "Laptop", Stock = 10 },
                new InventoryItem { Id = Guid.NewGuid(), ProductName = "Headphones", Stock = 50 },
                new InventoryItem { Id = Guid.NewGuid(), ProductName = "Coffee Maker", Stock = 20 }
            );
        }
    }
}
