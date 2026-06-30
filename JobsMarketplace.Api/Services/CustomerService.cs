using JobsMarketplace.Api.Common;
using JobsMarketplace.Api.Caching;
using JobsMarketplace.Api.Dtos.Customers;
using JobsMarketplace.Api.Exceptions;
using JobsMarketplace.Api.Repositories.Interfaces;
using JobsMarketplace.Api.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace JobsMarketplace.Api.Services;

public class CustomerService(
    ICustomerRepository customerRepository,
    IMemoryCache memoryCache,
    IDistributedCache distributedCache) : ICustomerService
{
    private static readonly MemoryCacheEntryOptions CacheOptions = new()
    {
        SlidingExpiration = TimeSpan.FromMinutes(5),
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15),
        Size = 1
    };
    private static readonly DistributedCacheEntryOptions DistributedCacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15),
        SlidingExpiration = TimeSpan.FromMinutes(5)
    };

    public async Task<CustomerResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"customer:{id}";

        if (memoryCache.TryGetValue(cacheKey, out CustomerResponseDto? cachedCustomer) && cachedCustomer is not null)
        {
            return cachedCustomer;
        }

        cachedCustomer = await distributedCache.GetAsync<CustomerResponseDto>(cacheKey, cancellationToken);
        if (cachedCustomer is not null)
        {
            memoryCache.Set(cacheKey, cachedCustomer, CacheOptions);
            return cachedCustomer;
        }

        var customer = await customerRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Customer with id {id} was not found.");

        var response = Map(customer);
        memoryCache.Set(cacheKey, response, CacheOptions);
        await distributedCache.SetAsync(cacheKey, response, DistributedCacheOptions, cancellationToken);
        return response;
    }

    public async Task<PagedResult<CustomerResponseDto>> SearchAsync(string? query, int page, int pageSize, string? cursor = null, CancellationToken cancellationToken = default)
    {
        ValidateSearchInputs(query, page, pageSize);
        var customers = await customerRepository.SearchAsync(query, page, pageSize, cursor, cancellationToken);

        return new PagedResult<CustomerResponseDto>
        {
            Items = customers.Items.Select(Map).ToList(),
            Page = customers.Page,
            PageSize = customers.PageSize,
            TotalCount = customers.TotalCount,
            NextCursor = customers.NextCursor
        };
    }

    private static void ValidateSearchInputs(string? query, int page, int pageSize)
    {
        if (page <= 0)
        {
            throw new RequestValidationException("Page must be greater than zero.");
        }

        if (pageSize <= 0 || pageSize > 100)
        {
            throw new RequestValidationException("PageSize must be between 1 and 100.");
        }

        if (!string.IsNullOrWhiteSpace(query) && query.Trim().Length > 100)
        {
            throw new RequestValidationException("Query must not exceed 100 characters.");
        }
    }

    private static CustomerResponseDto Map(Models.Customer customer) => new()
    {
        Id = customer.Id,
        FirstName = customer.FirstName,
        LastName = customer.LastName
    };
}
