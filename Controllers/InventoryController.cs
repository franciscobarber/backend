using Microsoft.AspNetCore.Mvc;
using RetailDemo.Data;

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

        [HttpGet("{productName}")]
        public IActionResult GetStock(string productName)
        {
            var item = _context.InventoryItems.FirstOrDefault(i => i.ProductName == productName);
            if (item == null) return NotFound();
            return Ok(item.Stock);
        }
    }
}
