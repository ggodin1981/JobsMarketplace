using JobsMarketplace.Api.Common;
using JobsMarketplace.Api.Dtos.Jobs;
using JobsMarketplace.Api.Exceptions;
using JobsMarketplace.Api.Models;
using JobsMarketplace.Api.Repositories.Interfaces;
using JobsMarketplace.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobsMarketplace.Api.Services;

public class JobService(ICustomerRepository customerRepository, IJobRepository jobRepository) : IJobService
{
    public async Task<PagedResult<JobResponseDto>> GetPagedAsync(int page, int pageSize, string? cursor = null, CancellationToken cancellationToken = default)
    {
        ValidatePaging(page, pageSize);

        var jobs = await jobRepository.GetPagedAsync(page, pageSize, cursor, cancellationToken);
        return new PagedResult<JobResponseDto>
        {
            Items = jobs.Items.Select(Map).ToList(),
            Page = jobs.Page,
            PageSize = jobs.PageSize,
            TotalCount = jobs.TotalCount,
            NextCursor = jobs.NextCursor
        };
    }

    public async Task<JobResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var job = await jobRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Job with id {id} was not found.");

        return Map(job);
    }

    public async Task<JobResponseDto> CreateAsync(int customerId, CreateJobRequestDto request, CancellationToken cancellationToken = default)
    {
        ValidateJob(request.StartDate, request.DueDate, request.Budget, request.Description);

        var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken);
        if (customer is null)
        {
            throw new NotFoundException($"Customer with id {customerId} was not found.");
        }

        var job = new Job
        {
            CustomerId = customerId,
            StartDate = request.StartDate,
            DueDate = request.DueDate,
            Budget = request.Budget,
            Description = request.Description.Trim(),
            Status = JobStatus.Open
        };

        await jobRepository.AddAsync(job, cancellationToken);
        await SaveChangesWithConcurrencyHandlingAsync(cancellationToken);

        return Map(job);
    }

    public async Task<JobResponseDto> UpdateAsync(int id, UpdateJobRequestDto request, CancellationToken cancellationToken = default)
    {
        ValidateJob(request.StartDate, request.DueDate, request.Budget, request.Description);

        var job = await jobRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Job with id {id} was not found.");

        if (job.Status != JobStatus.Open)
        {
            throw new RequestValidationException("Only open jobs can be updated.");
        }

        job.StartDate = request.StartDate;
        job.DueDate = request.DueDate;
        job.Budget = request.Budget;
        job.Description = request.Description.Trim();

        await SaveChangesWithConcurrencyHandlingAsync(cancellationToken);
        return Map(job);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var job = await jobRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Job with id {id} was not found.");

        if (job.Status != JobStatus.Open)
        {
            throw new RequestValidationException("Only open jobs can be deleted.");
        }

        jobRepository.Remove(job);
        await SaveChangesWithConcurrencyHandlingAsync(cancellationToken);
    }

    private static void ValidateJob(DateTime startDate, DateTime dueDate, decimal budget, string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new RequestValidationException("Description is required.");
        }

        if (budget <= 0)
        {
            throw new RequestValidationException("Budget must be greater than zero.");
        }

        if (dueDate <= startDate)
        {
            throw new RequestValidationException("Due date must be after start date.");
        }
    }

    private static void ValidatePaging(int page, int pageSize)
    {
        if (page <= 0)
        {
            throw new RequestValidationException("Page must be greater than zero.");
        }

        if (pageSize <= 0 || pageSize > 100)
        {
            throw new RequestValidationException("PageSize must be between 1 and 100.");
        }
    }

    private async Task SaveChangesWithConcurrencyHandlingAsync(CancellationToken cancellationToken)
    {
        try
        {
            await jobRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("The job was modified by another request. Please reload and try again.");
        }
    }

    private static JobResponseDto Map(Job job) => new()
    {
        Id = job.Id,
        CustomerId = job.CustomerId,
        StartDate = job.StartDate,
        DueDate = job.DueDate,
        Budget = job.Budget,
        Description = job.Description,
        AcceptedByContractorId = job.AcceptedByContractorId,
        Status = job.Status
    };
}
