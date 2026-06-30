using JobsMarketplace.Api.Common;
using JobsMarketplace.Api.Dtos.Jobs;

namespace JobsMarketplace.Api.Services.Interfaces;

public interface IJobService
{
    Task<PagedResult<JobResponseDto>> GetPagedAsync(int page, int pageSize, string? cursor = null, CancellationToken cancellationToken = default);
    Task<JobResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<JobResponseDto> CreateAsync(int customerId, CreateJobRequestDto request, CancellationToken cancellationToken = default);
    Task<JobResponseDto> UpdateAsync(int id, UpdateJobRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
