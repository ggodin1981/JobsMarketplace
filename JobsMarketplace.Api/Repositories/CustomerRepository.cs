using JobsMarketplace.Api.Common;
using JobsMarketplace.Api.Common.Cursors;
using JobsMarketplace.Api.Data;
using JobsMarketplace.Api.Models;
using JobsMarketplace.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobsMarketplace.Api.Repositories;

public class CustomerRepository(AppDbContext dbContext) : ICustomerRepository
{
    public Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<PagedResult<Customer>> SearchAsync(string? query, int page, int pageSize, string? cursor = null, CancellationToken cancellationToken = default)
    {
        var customers = dbContext.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query))
        {
            query = query.Trim();

            if (int.TryParse(query, out var id))
            {
                var exactCustomer = await customers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

                return new PagedResult<Customer>
                {
                    Items = exactCustomer is null ? Array.Empty<Customer>() : [exactCustomer],
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = exactCustomer is null ? 0 : 1
                };
            }

            customers = customers.Where(x => x.LastName.StartsWith(query));
        }

        var totalCount = await customers.CountAsync(cancellationToken);
        IQueryable<Customer> orderedCustomers = customers
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ThenBy(x => x.Id);

        var customerCursor = CursorSerializer.Deserialize<CustomerSearchCursor>(cursor);
        if (customerCursor is not null)
        {
            orderedCustomers = orderedCustomers.Where(x =>
                string.Compare(x.LastName, customerCursor.LastName) > 0
                || (x.LastName == customerCursor.LastName && string.Compare(x.FirstName, customerCursor.FirstName) > 0)
                || (x.LastName == customerCursor.LastName && x.FirstName == customerCursor.FirstName && x.Id > customerCursor.Id));
        }
        else
        {
            orderedCustomers = orderedCustomers.Skip((page - 1) * pageSize);
        }

        var items = await orderedCustomers
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        string? nextCursor = null;
        if (items.Count > pageSize)
        {
            var nextItem = items[pageSize - 1];
            nextCursor = CursorSerializer.Serialize(new CustomerSearchCursor(nextItem.LastName, nextItem.FirstName, nextItem.Id));
            items = items.Take(pageSize).ToList();
        }

        return new PagedResult<Customer>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            NextCursor = nextCursor
        };
    }
}
