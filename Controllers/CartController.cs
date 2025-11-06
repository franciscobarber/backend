using Microsoft.AspNetCore.Mvc;
using RetailDemo.Data;
using RetailDemo.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RetailDemo.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartController : ControllerBase
    {
        // In-memory store for carts. Key: userId, Value: List of CartItem.
        // This is static to persist across requests. A real implementation would use IMemoryCache or a distributed cache like Redis.
        private static readonly ConcurrentDictionary<string, List<CartItem>> _carts = new ConcurrentDictionary<string, List<CartItem>>();

        // We are commenting out the DbContext as we are switching to an in-memory cache.
        // private readonly RetailDbContext _context;

        public CartController(RetailDbContext context)
        {
            // _context = context;
        }

        [HttpGet("{userId}")]
        public IActionResult GetCartItems(string userId)
        {
            if (!_carts.TryGetValue(userId, out var items))
            {
                items = new List<CartItem>();
            }

            return Ok(new { items }); // Match the structure the frontend expects
        }

        [HttpPost("{userId}/items")]
        public IActionResult AddToCart(string userId, [FromBody] CartItem item)
        {
            var cart = _carts.GetOrAdd(userId, _ => new List<CartItem>());

            // For simplicity, we'll just add the item. A real implementation
            // would check if the product already exists and update the quantity.
            item.Id = Guid.NewGuid(); // Assign a new ID for this cart item entry.
            cart.Add(item);

            return Ok(item);
        }
    }
}
