using JobsMarketplace.Api.Models;

namespace JobsMarketplace.Api.Dtos.Jobs;

public class JobResponseDto
{
    public int Id { get; init; }
    public int CustomerId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime DueDate { get; init; }
    public decimal Budget { get; init; }
    public string Description { get; init; } = string.Empty;
    public int? AcceptedByContractorId { get; init; }
    public JobStatus Status { get; init; }
}

