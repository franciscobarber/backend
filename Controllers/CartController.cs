using Microsoft.AspNetCore.Mvc;
using RetailDemo.Data;
using RetailDemo.Dtos;
using Microsoft.Extensions.Logging;
using RetailDemo.Models;
using System.Collections.Concurrent;
using System.Linq;

namespace RetailDemo.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartController : ControllerBase
    {
        // In-memory store for carts. Key: userId, Value: <ProductId, Quantity>
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<System.Guid, int>> _carts = new ConcurrentDictionary<string, ConcurrentDictionary<System.Guid, int>>();
        // We are commenting out the DbContext as we are switching to an in-memory cache.
        private readonly ILogger<CartController> _logger;
        // private readonly RetailDbContext _context;

        public CartController(ILogger<CartController> logger)
        {
            // _context = context;
            _logger = logger;
        }

        [HttpGet("{userId}")]
        public IActionResult GetCartItems(string userId)
        {
            if (!_carts.TryGetValue(userId, out var cart))
            {
                // If the cart doesn't exist, return an empty list of items.
                return Ok(new { items = new System.Collections.Generic.List<object>() });
            }

            // Transform the dictionary into a list of objects for the frontend.
            var items = cart.Select(item => new { productId = item.Key, quantity = item.Value }).ToList();

            return Ok(new { items }); // Match the structure the frontend expects
        }

        [HttpPost("{userId}/items")]
        public IActionResult AddToCart(string userId, [FromBody] AddToCartRequest request)
        {
            if (request.Quantity <= 0)
            {
                return BadRequest("Quantity must be a positive number.");
            }

            var cart = _carts.GetOrAdd(userId, _ => new ConcurrentDictionary<System.Guid, int>());

            cart.AddOrUpdate(request.ProductId, request.Quantity, (key, oldQuantity) => oldQuantity + request.Quantity);

            return Ok(new { productId = request.ProductId, quantity = cart[request.ProductId] });
        }

        [HttpDelete("{userId}/items/{productId}")]
        public IActionResult RemoveFromCart(string userId, System.Guid productId)
        {
            if (_carts.TryGetValue(userId, out var cart))
            {
                cart.TryRemove(productId, out _);
            }
            return NoContent(); // Indicate success with no content to return.
        }

        [HttpDelete("{userId}")]
        public IActionResult ClearCart(string userId)
        {
            if (_carts.TryRemove(userId, out _))
            {
                _logger.LogInformation("Cart for user {UserId} cleared.", userId);
            }

            return NoContent();
        }
    }
}
