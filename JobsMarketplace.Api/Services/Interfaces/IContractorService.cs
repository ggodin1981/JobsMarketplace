using JobsMarketplace.Api.Common;
using JobsMarketplace.Api.Dtos.Contractors;

namespace JobsMarketplace.Api.Services.Interfaces;

public interface IContractorService
{
    Task<ContractorResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<ContractorResponseDto>> SearchAsync(string? query, int page, int pageSize, string? cursor = null, CancellationToken cancellationToken = default);
}
