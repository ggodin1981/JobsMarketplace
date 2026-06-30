using JobsMarketplace.Api.Data;
using JobsMarketplace.Api.Models;
using JobsMarketplace.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobsMarketplace.Api.Repositories;

public class JobOfferRepository(AppDbContext dbContext) : IJobOfferRepository
{
    public async Task<IReadOnlyList<JobOffer>> GetByJobIdAsync(int jobId, CancellationToken cancellationToken = default)
    {
        return await dbContext.JobOffers
            .AsNoTracking()
            .Where(x => x.JobId == jobId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<JobOffer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return dbContext.JobOffers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<JobOffer?> GetByIdWithJobAsync(int id, CancellationToken cancellationToken = default)
    {
        return dbContext.JobOffers
            .Include(x => x.Job)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<bool> ExistsForJobAndContractorAsync(int jobId, int contractorId, CancellationToken cancellationToken = default)
    {
        return dbContext.JobOffers.AnyAsync(
            x => x.JobId == jobId && x.ContractorId == contractorId,
            cancellationToken);
    }

    public Task AddAsync(JobOffer offer, CancellationToken cancellationToken = default)
    {
        return dbContext.JobOffers.AddAsync(offer, cancellationToken).AsTask();
    }

    public void Remove(JobOffer offer)
    {
        dbContext.JobOffers.Remove(offer);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}

