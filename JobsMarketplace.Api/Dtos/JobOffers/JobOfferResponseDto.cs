using JobsMarketplace.Api.Models;

namespace JobsMarketplace.Api.Dtos.JobOffers;

public class JobOfferResponseDto
{
    public int Id { get; init; }
    public int JobId { get; init; }
    public int ContractorId { get; init; }
    public decimal Price { get; init; }
    public JobOfferStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
}

