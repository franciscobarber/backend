using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using RetailDemo.Data;
using RetailDemo.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddControllers();
builder.WebHost.UseUrls("http://*:8080");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Use Azure SQL Database
builder.Services.AddDbContext<RetailDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AzureSql")));
builder.Services.AddMemoryCache();

var app = builder.Build();

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<RetailDbContext>();
    await dbContext.Database.MigrateAsync();
}

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
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

    try
    {
        var options = new DbContextOptionsBuilder<RetailDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        await using var db = new RetailDbContext(options);
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
