using JobsMarketplace.Api.Common;
using JobsMarketplace.Api.Dtos.Jobs;
using JobsMarketplace.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JobsMarketplace.Api.Controllers;

[ApiController]
[Route("jobs")]
public class JobsController(IJobService jobService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<JobResponseDto>), StatusCodes.Status200OK)]
    public Task<PagedResult<JobResponseDto>> GetAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? cursor = null,
        CancellationToken cancellationToken = default)
    {
        return jobService.GetPagedAsync(page, pageSize, cursor, cancellationToken);
    }

    [HttpGet("{id:int}", Name = "GetJobById")]
    [ProducesResponseType(typeof(JobResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<JobResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return jobService.GetByIdAsync(id, cancellationToken);
    }

    [HttpPost("/customers/{customerId:int}/jobs")]
    [ProducesResponseType(typeof(JobResponseDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<JobResponseDto>> CreateAsync(
        int customerId,
        [FromBody] CreateJobRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var response = await jobService.CreateAsync(customerId, request, cancellationToken);
        return CreatedAtRoute("GetJobById", new { id = response.Id }, response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(JobResponseDto), StatusCodes.Status200OK)]
    public Task<JobResponseDto> UpdateAsync(
        int id,
        [FromBody] UpdateJobRequestDto request,
        CancellationToken cancellationToken = default)
    {
        return jobService.UpdateAsync(id, request, cancellationToken);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await jobService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
