using System.ComponentModel.DataAnnotations;

namespace JobsMarketplace.Api.Dtos.Jobs;

public class UpdateJobRequestDto
{
    [Required]
    public DateTime StartDate { get; init; }

    [Required]
    public DateTime DueDate { get; init; }

    [Range(typeof(decimal), "0.01", "999999999999.99")]
    public decimal Budget { get; init; }

    [Required]
    [MaxLength(1000)]
    public string Description { get; init; } = string.Empty;
}

