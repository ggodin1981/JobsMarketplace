using JobsMarketplace.Api.Data;
using JobsMarketplace.Api.Dtos.Jobs;
using JobsMarketplace.Api.Exceptions;
using JobsMarketplace.Api.Models;
using JobsMarketplace.Api.Repositories;
using JobsMarketplace.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace JobsMarketplace.Tests;

public class JobServiceTests
{
    [Fact]
    public async Task CreateAsync_WithValidData_CreatesJob()
    {
        await using var dbContext = CreateContext();
        dbContext.Customers.Add(new Customer { Id = 1, FirstName = "John", LastName = "Smith" });
        await dbContext.SaveChangesAsync();

        var service = new JobService(new CustomerRepository(dbContext), new JobRepository(dbContext));

        var result = await service.CreateAsync(1, new CreateJobRequestDto
        {
            StartDate = new DateTime(2026, 7, 1),
            DueDate = new DateTime(2026, 7, 10),
            Budget = 1500m,
            Description = "Kitchen repair"
        });

        Assert.True(result.Id > 0);
        Assert.Equal(1, result.CustomerId);
        Assert.Equal(JobStatus.Open, result.Status);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidDates_ThrowsValidationException()
    {
        await using var dbContext = CreateContext();
        dbContext.Customers.Add(new Customer { Id = 1, FirstName = "John", LastName = "Smith" });
        await dbContext.SaveChangesAsync();

        var service = new JobService(new CustomerRepository(dbContext), new JobRepository(dbContext));

        var exception = await Assert.ThrowsAsync<RequestValidationException>(() => service.CreateAsync(1, new CreateJobRequestDto
        {
            StartDate = new DateTime(2026, 7, 10),
            DueDate = new DateTime(2026, 7, 1),
            Budget = 1500m,
            Description = "Kitchen repair"
        }));

        Assert.Equal("Due date must be after start date.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidBudget_ThrowsValidationException()
    {
        await using var dbContext = CreateContext();
        dbContext.Customers.Add(new Customer { Id = 1, FirstName = "John", LastName = "Smith" });
        await dbContext.SaveChangesAsync();

        var service = new JobService(new CustomerRepository(dbContext), new JobRepository(dbContext));

        var exception = await Assert.ThrowsAsync<RequestValidationException>(() => service.CreateAsync(1, new CreateJobRequestDto
        {
            StartDate = new DateTime(2026, 7, 1),
            DueDate = new DateTime(2026, 7, 10),
            Budget = 0m,
            Description = "Kitchen repair"
        }));

        Assert.Equal("Budget must be greater than zero.", exception.Message);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}

