using JobsMarketplace.Api.Dtos.JobOffers;
using JobsMarketplace.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JobsMarketplace.Api.Controllers;

[ApiController]
[Route("joboffers")]
public class JobOffersController(IJobOfferService jobOfferService) : ControllerBase
{
    [HttpGet("/jobs/{jobId:int}/offers")]
    [ProducesResponseType(typeof(IReadOnlyList<JobOfferResponseDto>), StatusCodes.Status200OK)]
    public Task<IReadOnlyList<JobOfferResponseDto>> GetByJobIdAsync(int jobId, CancellationToken cancellationToken = default)
    {
        return jobOfferService.GetByJobIdAsync(jobId, cancellationToken);
    }

    [HttpGet("{id:int}", Name = "GetJobOfferById")]
    [ProducesResponseType(typeof(JobOfferResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<JobOfferResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return jobOfferService.GetByIdAsync(id, cancellationToken);
    }

    [HttpPost("/jobs/{jobId:int}/offers")]
    [ProducesResponseType(typeof(JobOfferResponseDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<JobOfferResponseDto>> CreateAsync(
        int jobId,
        [FromBody] CreateJobOfferRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var response = await jobOfferService.CreateAsync(jobId, request, cancellationToken);
        return CreatedAtRoute("GetJobOfferById", new { id = response.Id }, response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(JobOfferResponseDto), StatusCodes.Status200OK)]
    public Task<JobOfferResponseDto> UpdateAsync(
        int id,
        [FromBody] UpdateJobOfferRequestDto request,
        CancellationToken cancellationToken = default)
    {
        return jobOfferService.UpdateAsync(id, request, cancellationToken);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await jobOfferService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("/jobs/{jobId:int}/offers/{offerId:int}/accept")]
    [ProducesResponseType(typeof(JobOfferResponseDto), StatusCodes.Status200OK)]
    public Task<JobOfferResponseDto> AcceptAsync(
        int jobId,
        int offerId,
        [FromBody] AcceptJobOfferRequestDto request,
        CancellationToken cancellationToken = default)
    {
        return jobOfferService.AcceptAsync(jobId, offerId, request, cancellationToken);
    }
}
