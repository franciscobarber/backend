using Microsoft.AspNetCore.Mvc;
using RetailDemo.Data;
using RetailDemo.Models;

namespace RetailDemo.Controllers
{
    [ApiController]
    [Route("api/order")]
    public class OrderController : ControllerBase
    {
        private readonly RetailDbContext _context;
        public OrderController(RetailDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult CreateOrder([FromBody] Order order)
        {
            order.OrderDate = DateTime.UtcNow;
            _context.Orders.Add(order);
            _context.SaveChanges();
            return Ok(order);
        }
    }
}
