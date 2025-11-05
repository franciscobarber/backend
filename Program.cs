using Microsoft.EntityFrameworkCore;
using RetailDemo.Data;
using RetailDemo.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Health Checks service, including a check for the DbContext
builder.Services.AddHealthChecks()
    .AddDbContextCheck<RetailDbContext>();

// Configure DbContext based on the environment
if (builder.Environment.IsDevelopment())
{
    // Use In-Memory Database for development/build if no DB is available
    builder.Services.AddDbContext<RetailDbContext>(options =>
        options.UseInMemoryDatabase("RetailDb"));
}
else
{
    // Register DbContext for Production (uses Azure SQL)
    builder.Services.AddDbContext<RetailDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("AzureSql")));
}

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

// Map the health check endpoint
app.MapHealthChecks("/healthz");

// Apply migrations automatically only in production
if (!app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        try
        {
            logger.LogInformation("Attempting to apply database migrations.");
            var db = services.GetRequiredService<RetailDbContext>();
            db.Database.Migrate();
            logger.LogInformation("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred during database migration.");
        }
    }
}

app.Run();
