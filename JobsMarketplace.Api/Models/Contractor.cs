namespace JobsMarketplace.Api.Models;

public class Contractor
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public ICollection<JobOffer> JobOffers { get; set; } = new List<JobOffer>();
}

