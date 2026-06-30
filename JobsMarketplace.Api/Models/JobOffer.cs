namespace JobsMarketplace.Api.Models;

public class JobOffer
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public int ContractorId { get; set; }
    public decimal Price { get; set; }
    public JobOfferStatus Status { get; set; } = JobOfferStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Job? Job { get; set; }
    public Contractor? Contractor { get; set; }
}

