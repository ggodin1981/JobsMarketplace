namespace JobsMarketplace.Api.Models;

public class Job
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Budget { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? AcceptedByContractorId { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Open;
    public uint Version { get; set; }

    public Customer? Customer { get; set; }
    public Contractor? AcceptedByContractor { get; set; }
    public ICollection<JobOffer> Offers { get; set; } = new List<JobOffer>();
}
