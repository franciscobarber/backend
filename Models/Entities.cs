namespace RetailDemo.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Category { get; set; }
        public decimal Price { get; set; }
    }

    public class InventoryItem
    {
        public Guid Id { get; set; }
        public int Stock { get; set; }

        // Foreign Key to Product
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }


public class Order
{
    public int Id { get; set; }
    public string? UserId { get; set; } // Add this property
    public DateTime OrderDate { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

public class OrderItem
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }

    // Foreign Key to Order
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    // Foreign Key to Product
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
}

    public class Recommendation
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}
