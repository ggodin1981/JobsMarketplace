using JobsMarketplace.Api.Common;
using JobsMarketplace.Api.Dtos.Customers;
using JobsMarketplace.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JobsMarketplace.Api.Controllers;

[ApiController]
[Route("customers")]
public class CustomersController(ICustomerService customerService) : ControllerBase
{
    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResult<CustomerResponseDto>), StatusCodes.Status200OK)]
    public Task<PagedResult<CustomerResponseDto>> SearchAsync(
        [FromQuery] string? query,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? cursor = null,
        CancellationToken cancellationToken = default)
    {
        return customerService.SearchAsync(query, page, pageSize, cursor, cancellationToken);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CustomerResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<CustomerResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return customerService.GetByIdAsync(id, cancellationToken);
    }
}
