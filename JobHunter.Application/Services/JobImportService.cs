
using JobHunter.Application.DTOs;
using JobHunter.Application.Enums;
using JobHunter.Application.Interfaces;
using JobHunter.Domain.Enums;

namespace JobHunter.Application.Services;

public sealed class JobImportService : IJobImportService
{
    private readonly IJobService _jobService;

    private readonly IReadOnlyDictionary<
        JobSourceType, IJobProvider> _providers;

    public JobImportService(
        IJobService jobService,
        IEnumerable<IJobProvider> providers)
    {
        _jobService = jobService
            ?? throw new ArgumentNullException(
                nameof(jobService));

        ArgumentNullException.ThrowIfNull(providers);

        _providers = providers.ToDictionary(
            provider => provider.SourceType);
    }

    public async Task<JobImportResult> ImportAsync(
        JobSourceType sourceType,
        JobProviderSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateRequest(request);

        cancellationToken.ThrowIfCancellationRequested();

        if (!_providers.TryGetValue(
                sourceType,
                out var provider))
        {
            throw new InvalidOperationException(
                $"No job provider registered for {sourceType}.");
        }

        var startedAt = DateTimeOffset.UtcNow;

        var jobs = await provider.SearchAsync(
            request,
            cancellationToken);

        var selectedJobs = jobs
            .Take(request.MaxResults)
            .ToList();

        var insertedCount = 0;
        var duplicateCount = 0;
        var addedSourceCount = 0;
        var possibleDuplicateCount = 0;
        var failedCount = 0;

        var errors = new List<JobImportError>();

        foreach (var job in selectedJobs)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var createRequest = new CreateJobRequest(
                    job.Title,
                    job.Company,
                    job.Location,
                    job.Description,
                    job.WorkArrangement,
                    sourceType,
                    job.SourceUrl,
                    job.ExternalId);

                var result = await _jobService.CreateAsync(
                    createRequest,
                    cancellationToken);

                if (result.WasCreated)
                {
                    insertedCount++;
                }
                else
                {
                    duplicateCount++;

                    if (result.SourceWasAdded)
                    {
                        addedSourceCount++;
                    }
                }

                if (result.MatchType ==
                    DuplicateMatchType.PossibleMatch)
                {
                    possibleDuplicateCount++;
                }
            }
            catch (ArgumentException exception)
            {
                failedCount++;

                errors.Add(new JobImportError(
                    job.ExternalId,
                    job.SourceUrl,
                    exception.Message));
            }
        }

        return new JobImportResult(
            sourceType,
            selectedJobs.Count,
            insertedCount,
            duplicateCount,
            addedSourceCount,
            possibleDuplicateCount,
            failedCount,
            startedAt,
            DateTimeOffset.UtcNow,
            errors.AsReadOnly());
    }

    private static void ValidateRequest(
        JobProviderSearchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Keywords))
        {
            throw new ArgumentException(
                "Search keywords cannot be empty.",
                nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Location))
        {
            throw new ArgumentException(
                "Search location cannot be empty.",
                nameof(request));
        }

        if (request.MaxResults <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                "Maximum results must be greater than zero.");
        }
    }
}
