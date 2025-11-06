using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RetailDemo.Data;
using RetailDemo.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RetailDemo.Controllers
{
    [ApiController]
    [Route("api/catalog")]
    public class CatalogController : ControllerBase
    {
        private readonly RetailDbContext _context;
        private readonly IMemoryCache _cache;
        private const string ProductsCacheKey = "ProductsList";

        public CatalogController(RetailDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _cache.GetOrCreateAsync(ProductsCacheKey, async entry =>
            {
                // Configure cache options
                entry.SetSlidingExpiration(TimeSpan.FromMinutes(5)); // Keep in cache, reset expiration on access
                entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(30)); // Remove from cache after this time
 
                // If the products are not in the cache, get them from the database
                return await _context.Products.ToListAsync();
            });
 
            return Ok(products);
        }
    }
}
