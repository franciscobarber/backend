using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using RetailDemo.Data;
using RetailDemo.Models;
using RetailDemo.Dtos;

namespace RetailDemo.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/order")]
    public class OrderController : ControllerBase
    {
        private readonly RetailDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OrderController> _logger;

        public OrderController(RetailDbContext context, IHttpClientFactory httpClientFactory, ILogger<OrderController> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("User ID not found.");
            }
            order.UserId = userId;
            order.OrderDate = DateTime.UtcNow;

            var httpClient = _httpClientFactory.CreateClient();
            // In a real-world scenario, the base address would come from configuration
            httpClient.BaseAddress = new Uri($"{Request.Scheme}://{Request.Host}");

            // 1. Check stock and hold items
            foreach (var item in order.OrderItems)
            {
                try
                {
                    var response = await httpClient.GetAsync($"/api/inventory/{item.ProductId}");
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Failed to get stock for product {ProductId}. Status: {StatusCode}", item.ProductId, response.StatusCode);
                        return BadRequest($"Could not verify stock for product {item.ProductId}.");
                    }

                    var stockData = await response.Content.ReadFromJsonAsync<InventoryDto>();
                    if (stockData == null || stockData.Stock < item.Quantity)
                    {
                        return Conflict($"Not enough stock for product {item.ProductId}. Available: {stockData?.Stock}, Requested: {item.Quantity}");
                    }

                    // Diminish stock
                    var newStock = stockData.Stock - item.Quantity;
                    var updateStockRequest = new UpdateStockRequest { Stock = newStock };
                    var jsonContent = new StringContent(JsonSerializer.Serialize(updateStockRequest), Encoding.UTF8, "application/json");
                    var updateResponse = await httpClient.PutAsync($"/api/inventory/{item.ProductId}", jsonContent);

                    if (!updateResponse.IsSuccessStatusCode)
                    {
                        // TODO: Implement rollback logic for previously updated items
                        _logger.LogError("Failed to update stock for product {ProductId}. Status: {StatusCode}", item.ProductId, updateResponse.StatusCode);
                        return StatusCode(500, "Failed to update inventory. Order cancelled.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error communicating with inventory service for product {ProductId}", item.ProductId);
                    return StatusCode(500, "An error occurred while processing your order.");
                }
            }

            // 2. Call payment service (mocked)
            // TODO: Implement actual payment service call
            var paymentSuccessful = true; // Mocking successful payment
            if (!paymentSuccessful)
            {
                // TODO: Implement rollback logic for inventory
                return BadRequest("Payment failed. Order cancelled.");
            }

            // 3. Post the order
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // 4. Empty the shopping cart
            var cartResponse = await httpClient.DeleteAsync($"/api/cart/{userId}");
            if (!cartResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("Could not empty the cart for user {UserId} after order creation.", userId);
            }

            return Ok(order);
        }
    }
}
