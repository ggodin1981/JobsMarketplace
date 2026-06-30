using JobsMarketplace.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JobsMarketplace.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Contractor> Contractors => Set<Contractor>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<JobOffer> JobOffers => Set<JobOffer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => new { x.LastName, x.FirstName });
        });

        modelBuilder.Entity<Contractor>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Rating).HasPrecision(3, 2);
            entity.HasIndex(x => new { x.Name, x.Id });
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Budget).HasPrecision(18, 2);
            entity.Property(x => x.Description).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.Version).IsRowVersion();
            entity.HasIndex(x => new { x.CustomerId, x.Id });
            entity.HasIndex(x => new { x.Status, x.Id });
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.Jobs)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.AcceptedByContractor)
                .WithMany()
                .HasForeignKey(x => x.AcceptedByContractorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<JobOffer>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Price).HasPrecision(18, 2);
            entity.HasIndex(x => new { x.JobId, x.Status, x.CreatedAt });
            entity.HasIndex(x => x.ContractorId);
            entity.HasIndex(x => new { x.JobId, x.ContractorId }).IsUnique();
            entity.HasOne(x => x.Job)
                .WithMany(x => x.Offers)
                .HasForeignKey(x => x.JobId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Contractor)
                .WithMany(x => x.JobOffers)
                .HasForeignKey(x => x.ContractorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
