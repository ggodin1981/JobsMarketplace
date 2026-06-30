using JobsMarketplace.Api.Models;

namespace JobsMarketplace.Api.Repositories.Interfaces;

public interface IJobOfferRepository
{
    Task<IReadOnlyList<JobOffer>> GetByJobIdAsync(int jobId, CancellationToken cancellationToken = default);
    Task<JobOffer?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<JobOffer?> GetByIdWithJobAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsForJobAndContractorAsync(int jobId, int contractorId, CancellationToken cancellationToken = default);
    Task AddAsync(JobOffer offer, CancellationToken cancellationToken = default);
    void Remove(JobOffer offer);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

