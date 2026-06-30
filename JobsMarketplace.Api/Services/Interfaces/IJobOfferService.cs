using JobsMarketplace.Api.Dtos.JobOffers;

namespace JobsMarketplace.Api.Services.Interfaces;

public interface IJobOfferService
{
    Task<IReadOnlyList<JobOfferResponseDto>> GetByJobIdAsync(int jobId, CancellationToken cancellationToken = default);
    Task<JobOfferResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<JobOfferResponseDto> CreateAsync(int jobId, CreateJobOfferRequestDto request, CancellationToken cancellationToken = default);
    Task<JobOfferResponseDto> UpdateAsync(int id, UpdateJobOfferRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<JobOfferResponseDto> AcceptAsync(int jobId, int offerId, AcceptJobOfferRequestDto request, CancellationToken cancellationToken = default);
}

