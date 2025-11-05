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

    public class CartItem
    {
        public Guid Id { get; set; }
        public int Quantity { get; set; }

        // Foreign Key to Product
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }

    public class Order
    {
        public Guid Id { get; set; }
        public required string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class Recommendation
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}
