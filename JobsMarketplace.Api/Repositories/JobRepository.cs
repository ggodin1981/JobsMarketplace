using JobsMarketplace.Api.Common;
using JobsMarketplace.Api.Common.Cursors;
using JobsMarketplace.Api.Data;
using JobsMarketplace.Api.Models;
using JobsMarketplace.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobsMarketplace.Api.Repositories;

public class JobRepository(AppDbContext dbContext) : IJobRepository
{
    public async Task<PagedResult<Job>> GetPagedAsync(int page, int pageSize, string? cursor = null, CancellationToken cancellationToken = default)
    {
        var jobs = dbContext.Jobs.AsNoTracking();
        var totalCount = await jobs.CountAsync(cancellationToken);
        var jobCursor = CursorSerializer.Deserialize<JobCursor>(cursor);

        if (jobCursor is not null)
        {
            jobs = jobs.Where(x => x.Id > jobCursor.Id);
        }

        IQueryable<Job> orderedJobs = jobs.OrderBy(x => x.Id);
        if (jobCursor is null)
        {
            orderedJobs = orderedJobs.Skip((page - 1) * pageSize);
        }

        var items = await orderedJobs
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        string? nextCursor = null;
        if (items.Count > pageSize)
        {
            var nextItem = items[pageSize - 1];
            nextCursor = CursorSerializer.Serialize(new JobCursor(nextItem.Id));
            items = items.Take(pageSize).ToList();
        }

        return new PagedResult<Job>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            NextCursor = nextCursor
        };
    }

    public Task<Job?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return dbContext.Jobs.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Job?> GetByIdWithOffersAsync(int id, CancellationToken cancellationToken = default)
    {
        return dbContext.Jobs
            .Include(x => x.Offers)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task AddAsync(Job job, CancellationToken cancellationToken = default)
    {
        return dbContext.Jobs.AddAsync(job, cancellationToken).AsTask();
    }

    public void Remove(Job job)
    {
        dbContext.Jobs.Remove(job);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
