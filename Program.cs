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

// Configure DbContext for Azure SQL
builder.Services.AddDbContext<RetailDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AzureSql")));

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

app.MapGet("/conn", async (RetailDbContext db, ILogger<Program> logger) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        return canConnect
            ? Results.Ok("Connection to the database succeeded!")
            : Results.Problem("Connection to the database failed.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Connection to the database failed.");
        return Results.Problem($"An exception occurred while connecting to the database: {ex.Message}");
    }
});

app.Run();
