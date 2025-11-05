namespace RetailDemo.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
    }

    public class InventoryItem
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; }
        public int Stock { get; set; }
    }

    public class CartItem
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }

    public class Order
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class Recommendation
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; }
    }
}
