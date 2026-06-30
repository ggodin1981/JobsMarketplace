using JobsMarketplace.Api.Dtos.JobOffers;
using JobsMarketplace.Api.Exceptions;
using JobsMarketplace.Api.Models;
using JobsMarketplace.Api.Repositories.Interfaces;
using JobsMarketplace.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobsMarketplace.Api.Services;

public class JobOfferService(
    IContractorRepository contractorRepository,
    IJobRepository jobRepository,
    IJobOfferRepository jobOfferRepository) : IJobOfferService
{
    public async Task<IReadOnlyList<JobOfferResponseDto>> GetByJobIdAsync(int jobId, CancellationToken cancellationToken = default)
    {
        var job = await jobRepository.GetByIdAsync(jobId, cancellationToken)
            ?? throw new NotFoundException($"Job with id {jobId} was not found.");

        var offers = await jobOfferRepository.GetByJobIdAsync(job.Id, cancellationToken);
        return offers.Select(Map).ToList();
    }

    public async Task<JobOfferResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var offer = await jobOfferRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Job offer with id {id} was not found.");

        return Map(offer);
    }

    public async Task<JobOfferResponseDto> CreateAsync(int jobId, CreateJobOfferRequestDto request, CancellationToken cancellationToken = default)
    {
        ValidatePrice(request.Price);

        var job = await jobRepository.GetByIdAsync(jobId, cancellationToken)
            ?? throw new NotFoundException($"Job with id {jobId} was not found.");

        if (job.Status != JobStatus.Open)
        {
            throw new RequestValidationException("Offers can only be created for open jobs.");
        }

        var contractor = await contractorRepository.GetByIdAsync(request.ContractorId, cancellationToken);
        if (contractor is null)
        {
            throw new NotFoundException($"Contractor with id {request.ContractorId} was not found.");
        }

        if (await jobOfferRepository.ExistsForJobAndContractorAsync(jobId, request.ContractorId, cancellationToken))
        {
            throw new RequestValidationException("This contractor already has an offer for the job.");
        }

        var offer = new JobOffer
        {
            JobId = jobId,
            ContractorId = request.ContractorId,
            Price = request.Price,
            Status = JobOfferStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await jobOfferRepository.AddAsync(offer, cancellationToken);
        await SaveChangesWithConcurrencyHandlingAsync(cancellationToken);

        return Map(offer);
    }

    public async Task<JobOfferResponseDto> UpdateAsync(int id, UpdateJobOfferRequestDto request, CancellationToken cancellationToken = default)
    {
        ValidatePrice(request.Price);

        var offer = await jobOfferRepository.GetByIdWithJobAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Job offer with id {id} was not found.");

        if (offer.Job is null)
        {
            throw new RequestValidationException("The offer is not linked to a valid job.");
        }

        if (offer.Status != JobOfferStatus.Pending || offer.Job.Status != JobStatus.Open)
        {
            throw new RequestValidationException("Only pending offers on open jobs can be updated.");
        }

        offer.Price = request.Price;
        await SaveChangesWithConcurrencyHandlingAsync(cancellationToken);

        return Map(offer);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var offer = await jobOfferRepository.GetByIdWithJobAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Job offer with id {id} was not found.");

        if (offer.Job is null)
        {
            throw new RequestValidationException("The offer is not linked to a valid job.");
        }

        if (offer.Status != JobOfferStatus.Pending || offer.Job.Status != JobStatus.Open)
        {
            throw new RequestValidationException("Only pending offers on open jobs can be deleted.");
        }

        jobOfferRepository.Remove(offer);
        await SaveChangesWithConcurrencyHandlingAsync(cancellationToken);
    }

    public async Task<JobOfferResponseDto> AcceptAsync(int jobId, int offerId, AcceptJobOfferRequestDto request, CancellationToken cancellationToken = default)
    {
        var job = await jobRepository.GetByIdWithOffersAsync(jobId, cancellationToken)
            ?? throw new NotFoundException($"Job with id {jobId} was not found.");

        if (job.CustomerId != request.CustomerId)
        {
            throw new RequestValidationException("Only the job owner can accept an offer.");
        }

        if (job.Status != JobStatus.Open)
        {
            throw new RequestValidationException("Offers can only be accepted for open jobs.");
        }

        var selectedOffer = job.Offers.FirstOrDefault(x => x.Id == offerId);
        if (selectedOffer is null)
        {
            throw new NotFoundException($"Job offer with id {offerId} was not found for job {jobId}.");
        }

        if (selectedOffer.Status != JobOfferStatus.Pending)
        {
            throw new RequestValidationException("Only pending offers can be accepted.");
        }

        foreach (var offer in job.Offers)
        {
            offer.Status = offer.Id == offerId
                ? JobOfferStatus.Accepted
                : JobOfferStatus.Rejected;
        }

        job.AcceptedByContractorId = selectedOffer.ContractorId;
        job.Status = JobStatus.Accepted;

        try
        {
            await jobRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("The job was modified by another request. Please reload and try again.");
        }

        return Map(selectedOffer);
    }

    private static void ValidatePrice(decimal price)
    {
        if (price <= 0)
        {
            throw new RequestValidationException("Price must be greater than zero.");
        }
    }

    private static JobOfferResponseDto Map(JobOffer offer) => new()
    {
        Id = offer.Id,
        JobId = offer.JobId,
        ContractorId = offer.ContractorId,
        Price = offer.Price,
        Status = offer.Status,
        CreatedAt = offer.CreatedAt
    };

    private async Task SaveChangesWithConcurrencyHandlingAsync(CancellationToken cancellationToken)
    {
        try
        {
            await jobOfferRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("The related job or offer was modified by another request. Please reload and try again.");
        }
    }
}
