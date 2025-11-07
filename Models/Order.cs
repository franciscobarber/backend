using System.ComponentModel.DataAnnotations;

namespace RetailDemo.Models
{
    public class Order
    {
        public Guid Id { get; set; }
        public string? UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public string? CartId { get; set; }
    }

    public class OrderItem
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}