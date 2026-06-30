using JobsMarketplace.Api.Data;
using JobsMarketplace.Api.Dtos.JobOffers;
using JobsMarketplace.Api.Exceptions;
using JobsMarketplace.Api.Models;
using JobsMarketplace.Api.Repositories;
using JobsMarketplace.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace JobsMarketplace.Tests;

public class JobOfferServiceTests
{
    [Fact]
    public async Task CreateAsync_WithValidData_CreatesOffer()
    {
        await using var dbContext = CreateContext();
        SeedOpenJobScenario(dbContext);

        var service = CreateService(dbContext);

        var result = await service.CreateAsync(1, new CreateJobOfferRequestDto
        {
            ContractorId = 1,
            Price = 900m
        });

        Assert.True(result.Id > 0);
        Assert.Equal(JobOfferStatus.Pending, result.Status);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidPrice_ThrowsValidationException()
    {
        await using var dbContext = CreateContext();
        SeedOpenJobScenario(dbContext);

        var service = CreateService(dbContext);

        var exception = await Assert.ThrowsAsync<RequestValidationException>(() => service.CreateAsync(1, new CreateJobOfferRequestDto
        {
            ContractorId = 1,
            Price = 0m
        }));

        Assert.Equal("Price must be greater than zero.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ForAcceptedJob_ThrowsValidationException()
    {
        await using var dbContext = CreateContext();
        SeedOpenJobScenario(dbContext);

        var job = await dbContext.Jobs.FirstAsync();
        job.Status = JobStatus.Accepted;
        job.AcceptedByContractorId = 1;
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var exception = await Assert.ThrowsAsync<RequestValidationException>(() => service.CreateAsync(1, new CreateJobOfferRequestDto
        {
            ContractorId = 1,
            Price = 900m
        }));

        Assert.Equal("Offers can only be created for open jobs.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_WithExistingOfferForSameContractor_ThrowsValidationException()
    {
        await using var dbContext = CreateContext();
        SeedOpenJobScenario(dbContext);
        dbContext.JobOffers.Add(new JobOffer
        {
            Id = 10,
            JobId = 1,
            ContractorId = 1,
            Price = 850m,
            Status = JobOfferStatus.Pending,
            CreatedAt = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var exception = await Assert.ThrowsAsync<RequestValidationException>(() => service.CreateAsync(1, new CreateJobOfferRequestDto
        {
            ContractorId = 1,
            Price = 900m
        }));

        Assert.Equal("This contractor already has an offer for the job.", exception.Message);
    }

    [Fact]
    public async Task AcceptAsync_WithValidCustomer_AcceptsSelectedOfferAndRejectsOthers()
    {
        await using var dbContext = CreateContext();
        SeedOfferAcceptanceScenario(dbContext);

        var service = CreateService(dbContext);

        var accepted = await service.AcceptAsync(1, 2, new AcceptJobOfferRequestDto
        {
            CustomerId = 1
        });

        Assert.Equal(JobOfferStatus.Accepted, accepted.Status);

        var offers = await dbContext.JobOffers.OrderBy(x => x.Id).ToListAsync();
        Assert.Equal(JobOfferStatus.Rejected, offers[0].Status);
        Assert.Equal(JobOfferStatus.Accepted, offers[1].Status);

        var job = await dbContext.Jobs.FirstAsync();
        Assert.Equal(JobStatus.Accepted, job.Status);
        Assert.Equal(2, job.AcceptedByContractorId);
    }

    [Fact]
    public async Task AcceptAsync_ForAnotherCustomersJob_ThrowsValidationException()
    {
        await using var dbContext = CreateContext();
        SeedOfferAcceptanceScenario(dbContext);

        var service = CreateService(dbContext);

        var exception = await Assert.ThrowsAsync<RequestValidationException>(() => service.AcceptAsync(1, 1, new AcceptJobOfferRequestDto
        {
            CustomerId = 99
        }));

        Assert.Equal("Only the job owner can accept an offer.", exception.Message);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static JobOfferService CreateService(AppDbContext dbContext)
    {
        return new JobOfferService(
            new ContractorRepository(dbContext),
            new JobRepository(dbContext),
            new JobOfferRepository(dbContext));
    }

    private static void SeedOpenJobScenario(AppDbContext dbContext)
    {
        dbContext.Customers.Add(new Customer { Id = 1, FirstName = "John", LastName = "Smith" });
        dbContext.Contractors.Add(new Contractor { Id = 1, Name = "BuildRight", Rating = 4.7m });
        dbContext.Jobs.Add(new Job
        {
            Id = 1,
            CustomerId = 1,
            StartDate = new DateTime(2026, 7, 1),
            DueDate = new DateTime(2026, 7, 10),
            Budget = 1500m,
            Description = "Kitchen repair",
            Status = JobStatus.Open
        });
        dbContext.SaveChanges();
    }

    private static void SeedOfferAcceptanceScenario(AppDbContext dbContext)
    {
        dbContext.Customers.Add(new Customer { Id = 1, FirstName = "John", LastName = "Smith" });
        dbContext.Contractors.AddRange(
            new Contractor { Id = 1, Name = "BuildRight", Rating = 4.7m },
            new Contractor { Id = 2, Name = "Skyline Electric", Rating = 4.9m });
        dbContext.Jobs.Add(new Job
        {
            Id = 1,
            CustomerId = 1,
            StartDate = new DateTime(2026, 7, 1),
            DueDate = new DateTime(2026, 7, 10),
            Budget = 1500m,
            Description = "Kitchen repair",
            Status = JobStatus.Open
        });
        dbContext.JobOffers.AddRange(
            new JobOffer { Id = 1, JobId = 1, ContractorId = 1, Price = 950m, Status = JobOfferStatus.Pending, CreatedAt = DateTime.UtcNow.AddMinutes(-10) },
            new JobOffer { Id = 2, JobId = 1, ContractorId = 2, Price = 920m, Status = JobOfferStatus.Pending, CreatedAt = DateTime.UtcNow });
        dbContext.SaveChanges();
    }
}
