using JobsMarketplace.Api.Common;
using JobsMarketplace.Api.Dtos.Contractors;
using JobsMarketplace.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JobsMarketplace.Api.Controllers;

[ApiController]
[Route("contractors")]
public class ContractorsController(IContractorService contractorService) : ControllerBase
{
    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResult<ContractorResponseDto>), StatusCodes.Status200OK)]
    public Task<PagedResult<ContractorResponseDto>> SearchAsync(
        [FromQuery] string? query,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? cursor = null,
        CancellationToken cancellationToken = default)
    {
        return contractorService.SearchAsync(query, page, pageSize, cursor, cancellationToken);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ContractorResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ContractorResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return contractorService.GetByIdAsync(id, cancellationToken);
    }
}
