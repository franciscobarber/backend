using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using RetailDemo.Data;
using RetailDemo.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    // This is the simplest way to configure a multi-tenant app.
    // Microsoft.Identity.Web will handle the issuer validation for "common", "organizations",
    // and personal accounts based on your appsettings.json.
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddControllers();
builder.WebHost.UseUrls("http://*:8080");

builder.Services.AddEndpointsApiExplorer();

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
// Apply migrations on startup - this is okay for development, but for production
// it's better to use a dedicated migration strategy (e.g., EF Core migrations CLI or a CI/CD step).
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<RetailDbContext>();
    try
    {
        await dbContext.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

// The order of middleware is important. CORS must be configured before Authentication/Authorization.
app.UseCors("AllowSpecificOrigin");
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
