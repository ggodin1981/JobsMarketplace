using JobsMarketplace.Api.Common;
using JobsMarketplace.Api.Models;

namespace JobsMarketplace.Api.Repositories.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<Customer>> SearchAsync(string? query, int page, int pageSize, string? cursor = null, CancellationToken cancellationToken = default);
}
