
using JobHunter.Application.DTOs;
using JobHunter.Application.Enums;
using JobHunter.Application.Interfaces;
using JobHunter.Domain.Entities;
using JobHunter.Domain.Enums;

namespace JobHunter.Application.Services;

public sealed class JobService : IJobService
{
    private readonly IJobRepository _repository;
    private readonly IDuplicateDetectionService _duplicateService;

    public JobService(
        IJobRepository repository,
        IDuplicateDetectionService duplicateService)
    {
        _repository = repository
            ?? throw new ArgumentNullException(nameof(repository));

        _duplicateService = duplicateService
            ?? throw new ArgumentNullException(
                nameof(duplicateService));
    }

    public async Task<CreateJobResult> CreateAsync(
        CreateJobRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Validate the input before performing duplicate detection.
        var newJob = new JobPosting(
            request.Title,
            request.Company,
            request.Location,
            request.Description,
            request.WorkArrangement);

        var newSource = new JobPostingSource(
            request.SourceType,
            request.SourceUrl,
            request.ExternalId);

        var duplicateRequest = new DuplicateCheckRequest(
            request.SourceType,
            request.ExternalId,
            request.SourceUrl,
            request.Title,
            request.Company,
            request.Location);

        var duplicate = await _duplicateService.CheckAsync(
            duplicateRequest,
            cancellationToken);

        if (duplicate.IsConfirmedDuplicate)
        {
            if (duplicate.ExistingJobId is not Guid existingId)
            {
                throw new InvalidOperationException(
                    "Confirmed duplicate has no existing job ID.");
            }

            var existingJob = await _repository.GetByIdAsync(
                existingId,
                cancellationToken);

            if (existingJob is null)
            {
                throw new InvalidOperationException(
                    "Duplicate detection referenced a missing job.");
            }

            var alreadyLinked = existingJob.Sources.Any(source =>
                source.SourceType == newSource.SourceType &&
                (source.NormalizedUrl == newSource.NormalizedUrl ||
                 (!string.IsNullOrWhiteSpace(newSource.ExternalId) &&
                  source.ExternalId == newSource.ExternalId)));

            if (!alreadyLinked)
            {
                existingJob.AddSource(newSource);

                await _repository.SaveChangesAsync(
                    cancellationToken);
            }

            return new CreateJobResult(
                MapToDto(existingJob),
                WasCreated: false,
                SourceWasAdded: !alreadyLinked,
                MatchType: duplicate.MatchType,
                PossibleDuplicateJobId: null);
        }

        newJob.AddSource(newSource);

        await _repository.AddAsync(
            newJob,
            cancellationToken);

        await _repository.SaveChangesAsync(
            cancellationToken);

        return new CreateJobResult(
            MapToDto(newJob),
            WasCreated: true,
            SourceWasAdded: true,
            MatchType: duplicate.MatchType,
            PossibleDuplicateJobId:
                duplicate.IsPossibleDuplicate
                    ? duplicate.ExistingJobId
                    : null);
    }

    public async Task<JobDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        ValidateId(id);

        var job = await _repository.GetByIdAsync(
            id,
            cancellationToken);

        return job is null ? null : MapToDto(job);
    }

    public async Task<IReadOnlyList<JobDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var jobs = await _repository.GetAllAsync(
            cancellationToken);

        return jobs.Select(MapToDto).ToList();
    }

    public async Task<bool> ChangeStatusAsync(
        Guid id,
        ApplicationStatus status,
        CancellationToken cancellationToken = default)
    {
        ValidateId(id);

        var job = await _repository.GetByIdAsync(
            id, cancellationToken);

        if (job is null)
            return false;

        job.ChangeStatus(status);

        await _repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ArchiveAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        ValidateId(id);

        var job = await _repository.GetByIdAsync(
            id, cancellationToken);

        if (job is null)
            return false;

        job.Archive();

        await _repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RestoreAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        ValidateId(id);

        var job = await _repository.GetByIdAsync(
            id, cancellationToken);

        if (job is null)
            return false;

        job.Restore();

        await _repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static void ValidateId(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid job ID is required.",
                nameof(id));
        }
    }

    private static JobDto MapToDto(JobPosting job)
    {
        var sources = job.Sources
            .Select(source => new JobSourceDto(
                source.Id,
                source.SourceType,
                source.OriginalUrl,
                source.ExternalId))
            .ToList();

        return new JobDto(
            job.Id,
            job.Title,
            job.Company,
            job.Location,
            job.Description,
            job.WorkArrangement,
            job.Status,
            job.DiscoveredAt,
            job.IsArchived,
            sources);
    }
}
