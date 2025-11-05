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

// Use In-Memory Database for the main application
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

app.MapGet("/conn", async (IConfiguration config, ILogger<Program> logger) =>
{
    var connectionString = config.GetConnectionString("AzureSql");
    var responseMessage = $"Attempting to connect with connection string: '{connectionString}'\n\n";

    try
    {
        var options = new DbContextOptionsBuilder<RetailDbContext>()
            .UseSqlServer(config.GetConnectionString("AzureSql"))
            .UseSqlServer(connectionString)
            .Options;
        await using var db = new RetailDbContext(options);
        var canConnect = await db.Database.CanConnectAsync();
        return canConnect
            ? Results.Ok("Connection to the database succeeded!")
            : Results.Problem("Connection to the database failed.");

        responseMessage += canConnect ? "Connection established!" : "Connection failed.";
        return Results.Ok(responseMessage);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Connection to the database failed.");
        return Results.Problem($"An exception occurred while connecting to the database: {ex.Message}");
        responseMessage += $"Connection failed with an error: {ex.Message}";
        return Results.Problem(responseMessage);
    }
});

app.Run();
