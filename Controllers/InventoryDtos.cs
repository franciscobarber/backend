using System.ComponentModel.DataAnnotations;

namespace RetailDemo.Dtos
{
    public class UpdateStockRequest
    {
        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }
    }

    public record InventoryDto(Guid ProductId, int Stock);
}