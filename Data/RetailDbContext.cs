using Microsoft.EntityFrameworkCore;
using RetailDemo.Models;

namespace RetailDemo.Data
{
    public class RetailDbContext : DbContext
    {
        public RetailDbContext(DbContextOptions<RetailDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Recommendation> Recommendations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Define deterministic Guids for seed data
            var laptopId = Guid.Parse("9A477379-320D-4A43-A47B-3058887F5F3C");
            var headphonesId = Guid.Parse("AE80925F-441D-449A-A14A-528656594867");
            var coffeeMakerId = Guid.Parse("F7317540-7A8C-49A9-9A3C-90A0E759A2D4");

            // Seed Products
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = laptopId, Name = "Laptop", Category = "Electronics", Price = 1200 },
                new Product { Id = headphonesId, Name = "Headphones", Category = "Electronics", Price = 150 },
                new Product { Id = coffeeMakerId, Name = "Coffee Maker", Category = "Home Appliances", Price = 80 }
            );

            // Seed Inventory
            modelBuilder.Entity<InventoryItem>().HasData(
                new InventoryItem { Id = Guid.Parse("C9A5B9A7-5D1F-4B0E-9D2A-1B3E4C6F8A9B"), ProductId = laptopId, Stock = 10 },
                new InventoryItem { Id = Guid.Parse("D8B6C8B6-6E2F-4C1F-AF3B-2C4F5D7E9B8D"), ProductId = headphonesId, Stock = 50 },
                new InventoryItem { Id = Guid.Parse("E7C7D7C5-7F3F-4D2E-BE4C-3D5E6F8DAB7E"), ProductId = coffeeMakerId, Stock = 20 }
            );
        }
    }
}
