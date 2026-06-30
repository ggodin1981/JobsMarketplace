using System.ComponentModel.DataAnnotations;

namespace JobsMarketplace.Api.Dtos.JobOffers;

public class AcceptJobOfferRequestDto
{
    [Range(1, int.MaxValue)]
    public int CustomerId { get; init; }
}

