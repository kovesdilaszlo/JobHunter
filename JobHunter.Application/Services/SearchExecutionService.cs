
using JobHunter.Application.DTOs;
using JobHunter.Application.Interfaces;
using JobHunter.Domain.Entities;
using JobHunter.Domain.Enums;

namespace JobHunter.Application.Services;

public sealed class SearchExecutionService
    : ISearchExecutionService
{
    private readonly IJobSearchProfileRepository
        _profileRepository;

    private readonly ISearchExecutionRepository
        _executionRepository;

    private readonly ISearchImportRunner _importRunner;

    public SearchExecutionService(
        IJobSearchProfileRepository profileRepository,
        ISearchExecutionRepository executionRepository,
        ISearchImportRunner importRunner)
    {
        _profileRepository = profileRepository
            ?? throw new ArgumentNullException(
                nameof(profileRepository));

        _executionRepository = executionRepository
            ?? throw new ArgumentNullException(
                nameof(executionRepository));

        _importRunner = importRunner
            ?? throw new ArgumentNullException(
                nameof(importRunner));
    }

    public async Task<SearchExecutionDto> ExecuteAsync(
        StartSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateRequest(request);

        cancellationToken.ThrowIfCancellationRequested();

        var profile =
            await _profileRepository.GetByIdAsync(
                request.SearchProfileId,
                cancellationToken);

        if (profile is null)
        {
            throw new KeyNotFoundException(
                "Search profile was not found.");
        }

        if (!profile.IsEnabled)
        {
            throw new InvalidOperationException(
                "The search profile is disabled.");
        }

        var providerRequest =
            new JobProviderSearchRequest(
                profile.Keywords,
                profile.Location ?? "Hungary",
                profile.IncludeRemote,
                request.MaxResults);

        var execution = new SearchExecution(
            profile.Id,
            request.SourceType.ToString(),
            request.TriggerType);

        await _executionRepository.AddAsync(
            execution,
            cancellationToken);

        execution.Start();

        await _executionRepository.SaveChangesAsync(
            cancellationToken);

        JobImportResult result;

        try
        {
            result = await _importRunner.RunAsync(
                request.SourceType,
                providerRequest,
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            execution.Cancel();

            await _executionRepository.SaveChangesAsync(
                CancellationToken.None);

            throw;
        }
        catch (Exception)
        {
            execution.Fail(
                "The import failed because of a provider or infrastructure error.");

            await _executionRepository.SaveChangesAsync(
                CancellationToken.None);

            throw;
        }

        ApplyImportResult(execution, result);

        await _executionRepository.SaveChangesAsync(
            CancellationToken.None);

        return MapToDto(execution);
    }

    public async Task<SearchExecutionDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid execution ID is required.",
                nameof(id));
        }

        var execution =
            await _executionRepository.GetByIdAsync(
                id,
                cancellationToken);

        return execution is null
            ? null
            : MapToDto(execution);
    }

    public async Task<IReadOnlyList<SearchExecutionDto>>
        GetRecentAsync(
            int count = 50,
            CancellationToken cancellationToken = default)
    {
        if (count <= 0 || count > 500)
        {
            throw new ArgumentOutOfRangeException(
                nameof(count),
                "Count must be between 1 and 500.");
        }

        var executions =
            await _executionRepository.GetRecentAsync(
                count,
                cancellationToken);

        return executions
            .Select(MapToDto)
            .ToList();
    }

    private static void ApplyImportResult(
        SearchExecution execution,
        JobImportResult result)
    {
        if (result.IsSuccessful)
        {
            execution.Complete(
                result.RetrievedCount,
                result.InsertedCount,
                result.DuplicateCount);

            return;
        }

        var errorMessage =
            $"{result.FailedCount} job posting(s) could not be imported.";

        if (result.IsPartiallySuccessful)
        {
            execution.CompletePartially(
                result.RetrievedCount,
                result.InsertedCount,
                result.DuplicateCount,
                errorMessage);

            return;
        }

        execution.Fail(
            errorMessage,
            result.RetrievedCount,
            result.InsertedCount,
            result.DuplicateCount);
    }

    private static void ValidateRequest(
        StartSearchRequest request)
    {
        if (request.SearchProfileId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid search profile ID is required.",
                nameof(request));
        }

        if (!Enum.IsDefined(request.SourceType) ||
            request.SourceType == JobSourceType.Manual)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                "Invalid automated search provider.");
        }

        if (!Enum.IsDefined(request.TriggerType))
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                "Invalid search trigger type.");
        }

        if (request.MaxResults <= 0 ||
            request.MaxResults > 1000)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                "MaxResults must be between 1 and 1000.");
        }
    }

    private static SearchExecutionDto MapToDto(
        SearchExecution execution)
    {
        return new SearchExecutionDto(
            execution.Id,
            execution.JobSearchProfileId,
            execution.ProviderName,
            execution.TriggerType,
            execution.Status,
            execution.StartedAt,
            execution.CompletedAt,
            execution.RetrievedCount,
            execution.InsertedCount,
            execution.DuplicateCount,
            execution.ErrorMessage);
    }
}
