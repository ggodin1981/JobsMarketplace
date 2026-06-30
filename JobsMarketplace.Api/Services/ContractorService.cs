using JobsMarketplace.Api.Common;
using JobsMarketplace.Api.Caching;
using JobsMarketplace.Api.Dtos.Contractors;
using JobsMarketplace.Api.Exceptions;
using JobsMarketplace.Api.Repositories.Interfaces;
using JobsMarketplace.Api.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace JobsMarketplace.Api.Services;

public class ContractorService(
    IContractorRepository contractorRepository,
    IMemoryCache memoryCache,
    IDistributedCache distributedCache) : IContractorService
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

    public async Task<ContractorResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"contractor:{id}";

        if (memoryCache.TryGetValue(cacheKey, out ContractorResponseDto? cachedContractor) && cachedContractor is not null)
        {
            return cachedContractor;
        }

        cachedContractor = await distributedCache.GetAsync<ContractorResponseDto>(cacheKey, cancellationToken);
        if (cachedContractor is not null)
        {
            memoryCache.Set(cacheKey, cachedContractor, CacheOptions);
            return cachedContractor;
        }

        var contractor = await contractorRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Contractor with id {id} was not found.");

        var response = Map(contractor);
        memoryCache.Set(cacheKey, response, CacheOptions);
        await distributedCache.SetAsync(cacheKey, response, DistributedCacheOptions, cancellationToken);
        return response;
    }

    public async Task<PagedResult<ContractorResponseDto>> SearchAsync(string? query, int page, int pageSize, string? cursor = null, CancellationToken cancellationToken = default)
    {
        ValidateSearchInputs(query, page, pageSize);
        var contractors = await contractorRepository.SearchAsync(query, page, pageSize, cursor, cancellationToken);

        return new PagedResult<ContractorResponseDto>
        {
            Items = contractors.Items.Select(Map).ToList(),
            Page = contractors.Page,
            PageSize = contractors.PageSize,
            TotalCount = contractors.TotalCount,
            NextCursor = contractors.NextCursor
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

    private static ContractorResponseDto Map(Models.Contractor contractor) => new()
    {
        Id = contractor.Id,
        Name = contractor.Name,
        Rating = contractor.Rating
    };
}
