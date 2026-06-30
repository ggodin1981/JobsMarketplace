using JobsMarketplace.Api.Data;
using JobsMarketplace.Api.Exceptions;
using JobsMarketplace.Api.Models;
using JobsMarketplace.Api.Repositories;
using JobsMarketplace.Api.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;

namespace JobsMarketplace.Tests;

public class SearchServiceTests
{
    [Fact]
    public async Task CustomerSearch_ReturnsMatchingLastNames()
    {
        await using var dbContext = CreateContext();
        SeedSearchData(dbContext);

        using var cache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 1024 });
        var service = new CustomerService(new CustomerRepository(dbContext), cache, CreateDistributedCache());

        var result = await service.SearchAsync("Sm", 1, 20);

        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, item => Assert.StartsWith("Sm", item.LastName));
    }

    [Fact]
    public async Task ContractorSearch_ReturnsMatchingNames()
    {
        await using var dbContext = CreateContext();
        SeedSearchData(dbContext);

        using var cache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 1024 });
        var service = new ContractorService(new ContractorRepository(dbContext), cache, CreateDistributedCache());

        var result = await service.SearchAsync("Build", 1, 20);

        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, item => Assert.StartsWith("Build", item.Name));
    }

    [Fact]
    public async Task CustomerSearch_WithCursor_ReturnsNextPage()
    {
        await using var dbContext = CreateContext();
        SeedSearchData(dbContext);
        dbContext.Customers.Add(new Customer { Id = 4, FirstName = "Sam", LastName = "Smalls" });
        dbContext.Customers.Add(new Customer { Id = 5, FirstName = "Zoe", LastName = "Smith" });
        await dbContext.SaveChangesAsync();

        using var cache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 1024 });
        var service = new CustomerService(new CustomerRepository(dbContext), cache, CreateDistributedCache());

        var firstPage = await service.SearchAsync("Sm", 1, 2);
        var secondPage = await service.SearchAsync("Sm", 1, 2, firstPage.NextCursor);

        Assert.NotNull(firstPage.NextCursor);
        Assert.Equal(2, firstPage.Items.Count);
        Assert.NotEmpty(secondPage.Items);
        Assert.DoesNotContain(secondPage.Items, item => firstPage.Items.Any(first => first.Id == item.Id));
    }

    [Fact]
    public async Task CustomerSearch_WithInvalidPage_ThrowsValidationException()
    {
        await using var dbContext = CreateContext();
        SeedSearchData(dbContext);

        using var cache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 1024 });
        var service = new CustomerService(new CustomerRepository(dbContext), cache, CreateDistributedCache());

        var exception = await Assert.ThrowsAsync<RequestValidationException>(() => service.SearchAsync("Sm", 0, 20));

        Assert.Equal("Page must be greater than zero.", exception.Message);
    }

    [Fact]
    public async Task ContractorSearch_WithInvalidPageSize_ThrowsValidationException()
    {
        await using var dbContext = CreateContext();
        SeedSearchData(dbContext);

        using var cache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 1024 });
        var service = new ContractorService(new ContractorRepository(dbContext), cache, CreateDistributedCache());

        var exception = await Assert.ThrowsAsync<RequestValidationException>(() => service.SearchAsync("Build", 1, 101));

        Assert.Equal("PageSize must be between 1 and 100.", exception.Message);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static void SeedSearchData(AppDbContext dbContext)
    {
        dbContext.Customers.AddRange(
            new Customer { Id = 1, FirstName = "John", LastName = "Smith" },
            new Customer { Id = 2, FirstName = "Jane", LastName = "Smythe" },
            new Customer { Id = 3, FirstName = "Alice", LastName = "Brown" });

        dbContext.Contractors.AddRange(
            new Contractor { Id = 1, Name = "BuildRight Services", Rating = 4.7m },
            new Contractor { Id = 2, Name = "BuildPro Works", Rating = 4.2m },
            new Contractor { Id = 3, Name = "Northwind Plumbing", Rating = 4.5m });

        dbContext.SaveChanges();
    }

    private static IDistributedCache CreateDistributedCache()
    {
        var services = new ServiceCollection();
        services.AddDistributedMemoryCache();
        return services.BuildServiceProvider().GetRequiredService<IDistributedCache>();
    }
}
