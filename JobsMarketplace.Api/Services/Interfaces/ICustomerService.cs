using JobsMarketplace.Api.Common;
using JobsMarketplace.Api.Dtos.Customers;

namespace JobsMarketplace.Api.Services.Interfaces;

public interface ICustomerService
{
    Task<CustomerResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<CustomerResponseDto>> SearchAsync(string? query, int page, int pageSize, string? cursor = null, CancellationToken cancellationToken = default);
}
