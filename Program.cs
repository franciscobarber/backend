using Microsoft.EntityFrameworkCore;
using RetailDemo.Data;
using Microsoft.Extensions.DependencyInjection;
using RetailDemo.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.WebHost.UseUrls("http://*:8080");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Use In-Memory Database for now
builder.Services.AddDbContext<RetailDbContext>(options =>
    options.UseInMemoryDatabase("RetailDb"));

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Map root endpoint to display table contents
// WARNING: This endpoint fetches all products and orders. This can cause performance issues or timeouts if the tables are large.
app.MapGet("/", async (RetailDbContext db) =>
{
    var products = await db.Products.ToListAsync();
    var orders = await db.Orders.ToListAsync();
    return Results.Ok(new { Products = products, Orders = orders });
});

app.Run();
