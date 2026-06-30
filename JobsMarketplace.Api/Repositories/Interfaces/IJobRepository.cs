using JobsMarketplace.Api.Common;
using JobsMarketplace.Api.Models;

namespace JobsMarketplace.Api.Repositories.Interfaces;

public interface IJobRepository
{
    Task<PagedResult<Job>> GetPagedAsync(int page, int pageSize, string? cursor = null, CancellationToken cancellationToken = default);
    Task<Job?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Job?> GetByIdWithOffersAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(Job job, CancellationToken cancellationToken = default);
    void Remove(Job job);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
