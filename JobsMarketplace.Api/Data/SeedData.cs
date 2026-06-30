using JobsMarketplace.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JobsMarketplace.Api.Data;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (!await context.Customers.AnyAsync())
        {
            context.Customers.AddRange(
                new Customer { Id = 1, FirstName = "John", LastName = "Smith" },
                new Customer { Id = 2, FirstName = "Maria", LastName = "Santos" },
                new Customer { Id = 3, FirstName = "Alice", LastName = "Brown" },
                new Customer { Id = 4, FirstName = "David", LastName = "Miller" });
        }

        if (!await context.Contractors.AnyAsync())
        {
            context.Contractors.AddRange(
                new Contractor { Id = 1, Name = "BuildRight Services", Rating = 4.70m },
                new Contractor { Id = 2, Name = "Northwind Plumbing", Rating = 4.40m },
                new Contractor { Id = 3, Name = "Skyline Electric", Rating = 4.90m },
                new Contractor { Id = 4, Name = "Crafted Interiors", Rating = 4.50m });
        }

        await context.SaveChangesAsync();
    }
}
