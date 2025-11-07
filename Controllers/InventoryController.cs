using Microsoft.AspNetCore.Mvc;
using RetailDemo.Data;
using RetailDemo.Dtos;
using Microsoft.EntityFrameworkCore;

namespace RetailDemo.Controllers
{
    [ApiController]
    [Route("api/inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly RetailDbContext _context;
        public InventoryController(RetailDbContext context)
        {
            _context = context;
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetStock(Guid productId)
        {
            var item = await _context.Inventory
                .FirstOrDefaultAsync(i => i.ProductId == productId);

            if (item == null)
            {
                return NotFound($"No inventory record found for product ID {productId}");
            }
            return Ok(new { productId = item.ProductId, stock = item.Stock });
        }

        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateStock(Guid productId, [FromBody] UpdateStockRequest request)
        {
            var item = await _context.Inventory
                .FirstOrDefaultAsync(i => i.ProductId == productId);

            if (item == null)
            {
                return NotFound($"No inventory record found for product ID {productId}");
            }

            item.Stock = request.Stock;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
