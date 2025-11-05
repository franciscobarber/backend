using Microsoft.AspNetCore.Mvc;
using RetailDemo.Data;
using RetailDemo.Models;

namespace RetailDemo.Controllers
{
    [ApiController]
    [Route("api/catalog")]
    public class CatalogController : ControllerBase
    {
        private readonly RetailDbContext _context;
        public CatalogController(RetailDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetProducts()
        {
            return Ok(_context.Products.ToList());
        }
    }
}
