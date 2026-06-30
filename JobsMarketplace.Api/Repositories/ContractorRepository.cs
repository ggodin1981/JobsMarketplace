using JobsMarketplace.Api.Common;
using JobsMarketplace.Api.Common.Cursors;
using JobsMarketplace.Api.Data;
using JobsMarketplace.Api.Models;
using JobsMarketplace.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobsMarketplace.Api.Repositories;

public class ContractorRepository(AppDbContext dbContext) : IContractorRepository
{
    public Task<Contractor?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return dbContext.Contractors
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<PagedResult<Contractor>> SearchAsync(string? query, int page, int pageSize, string? cursor = null, CancellationToken cancellationToken = default)
    {
        var contractors = dbContext.Contractors.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query))
        {
            query = query.Trim();

            if (int.TryParse(query, out var id))
            {
                var exactContractor = await contractors.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

                return new PagedResult<Contractor>
                {
                    Items = exactContractor is null ? Array.Empty<Contractor>() : [exactContractor],
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = exactContractor is null ? 0 : 1
                };
            }

            contractors = contractors.Where(x => x.Name.StartsWith(query));
        }

        var totalCount = await contractors.CountAsync(cancellationToken);
        IQueryable<Contractor> orderedContractors = contractors
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id);

        var contractorCursor = CursorSerializer.Deserialize<ContractorSearchCursor>(cursor);
        if (contractorCursor is not null)
        {
            orderedContractors = orderedContractors.Where(x =>
                string.Compare(x.Name, contractorCursor.Name) > 0
                || (x.Name == contractorCursor.Name && x.Id > contractorCursor.Id));
        }
        else
        {
            orderedContractors = orderedContractors.Skip((page - 1) * pageSize);
        }

        var items = await orderedContractors
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        string? nextCursor = null;
        if (items.Count > pageSize)
        {
            var nextItem = items[pageSize - 1];
            nextCursor = CursorSerializer.Serialize(new ContractorSearchCursor(nextItem.Name, nextItem.Id));
            items = items.Take(pageSize).ToList();
        }

        return new PagedResult<Contractor>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            NextCursor = nextCursor
        };
    }
}
