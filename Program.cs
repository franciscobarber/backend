using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Validators; // Add this using directive
using RetailDemo.Data;
using RetailDemo.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(
        jwtOptions => // Configure JwtBearerOptions
        {
            builder.Configuration.Bind("AzureAd", jwtOptions);

            // Explicitly configure TokenValidationParameters for multi-tenant issuer validation
            jwtOptions.TokenValidationParameters.ValidateIssuer = true; // Ensure issuer validation is enabled

            // Get the base Azure AD instance URL from configuration (e.g., "https://login.microsoftonline.com/")
            var azureAdInstance = builder.Configuration["AzureAd:Instance"];
            // Use AadIssuerValidator for multi-tenant scenarios to dynamically validate the issuer
            jwtOptions.TokenValidationParameters.IssuerValidator =
                AadIssuerValidator.GetAadIssuerValidator(azureAdInstance).Validate;
        },
        microsoftIdentityOptions => // Configure MicrosoftIdentityOptions
        {
            builder.Configuration.Bind("AzureAd", microsoftIdentityOptions);
        });

builder.Services.AddControllers();
builder.WebHost.UseUrls("http://*:8080");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    // Add JWT Bearer authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            new string[] { }
        }
    });
});

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
