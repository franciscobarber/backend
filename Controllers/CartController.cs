using Microsoft.AspNetCore.Mvc;
using RetailDemo.Data;
using RetailDemo.Models;

namespace RetailDemo.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartController : ControllerBase
    {
        private readonly RetailDbContext _context;
        public CartController(RetailDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetCartItems()
        {
            return Ok(_context.CartItems.ToList());
        }

        [HttpPost]
        public IActionResult AddToCart([FromBody] CartItem item)
        {
            _context.CartItems.Add(item);
            _context.SaveChanges();
            return Ok(item);
        }
    }
}
