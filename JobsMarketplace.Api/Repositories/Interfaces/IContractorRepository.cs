using JobsMarketplace.Api.Common;
using JobsMarketplace.Api.Models;

namespace JobsMarketplace.Api.Repositories.Interfaces;

public interface IContractorRepository
{
    Task<Contractor?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<Contractor>> SearchAsync(string? query, int page, int pageSize, string? cursor = null, CancellationToken cancellationToken = default);
}
