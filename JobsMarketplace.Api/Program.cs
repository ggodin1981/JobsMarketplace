using JobsMarketplace.Api.Data;
using JobsMarketplace.Api.HealthChecks;
using JobsMarketplace.Api.Middleware;
using JobsMarketplace.Api.Repositories;
using JobsMarketplace.Api.Repositories.Interfaces;
using JobsMarketplace.Api.Services;
using JobsMarketplace.Api.Services.Interfaces;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("The DefaultConnection connection string is not configured.");
var redisConnectionString = builder.Configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException("The Redis connection string is not configured.");

builder.Services.AddControllers();
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestMethod
        | HttpLoggingFields.RequestPath
        | HttpLoggingFields.ResponseStatusCode
        | HttpLoggingFields.Duration;
});
builder.Services.AddMemoryCache(options => options.SizeLimit = 1024);
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "JobsMarketplace:";
});
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IContractorRepository, ContractorRepository>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IJobOfferRepository, JobOfferRepository>();

builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IContractorService, ContractorService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IJobOfferService, JobOfferService>();
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("postgres")
    .AddCheck<RedisHealthCheck>("redis");

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpLogging();
app.UseMiddleware<RequestTimingMiddleware>();
app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/health"),
    applicationBuilder => applicationBuilder.UseHttpsRedirection());

app.MapControllers();
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = _ => true
});

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await InitializeDatabaseAsync(dbContext);
}

await app.RunAsync();

static async Task InitializeDatabaseAsync(AppDbContext dbContext)
{
    const int maxAttempts = 5;

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            if (await dbContext.Database.CanConnectAsync())
            {
                break;
            }
        }
        catch when (attempt < maxAttempts)
        {
        }

        if (attempt == maxAttempts)
        {
            throw new InvalidOperationException("The database did not become reachable in time.");
        }

        await Task.Delay(TimeSpan.FromSeconds(2 * attempt));
    }

    await SeedData.InitializeAsync(dbContext);
}

public partial class Program;
