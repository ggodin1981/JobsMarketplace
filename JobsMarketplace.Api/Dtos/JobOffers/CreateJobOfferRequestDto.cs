using System.ComponentModel.DataAnnotations;

namespace JobsMarketplace.Api.Dtos.JobOffers;

public class CreateJobOfferRequestDto
{
    [Range(1, int.MaxValue)]
    public int ContractorId { get; init; }

    [Range(typeof(decimal), "0.01", "999999999999.99")]
    public decimal Price { get; init; }
}

