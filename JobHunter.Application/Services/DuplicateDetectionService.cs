
using System.Text.RegularExpressions;
using JobHunter.Application.DTOs;
using JobHunter.Application.Enums;
using JobHunter.Application.Interfaces;
using JobHunter.Domain.Utilities;

namespace JobHunter.Application.Services;

public sealed class DuplicateDetectionService
    : IDuplicateDetectionService
{
    private readonly IJobRepository _repository;

    public DuplicateDetectionService(
        IJobRepository repository)
    {
        _repository = repository
            ?? throw new ArgumentNullException(
                nameof(repository));
    }

    public async Task<DuplicateCheckResult> CheckAsync(
        DuplicateCheckRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!Enum.IsDefined(request.SourceType))
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                "Invalid source type.");
        }

        var normalizedUrl =
            UrlNormalizer.Normalize(request.SourceUrl);

        var fingerprint =
            UrlNormalizer.CreateFingerprint(request.SourceUrl);

        var jobs = await _repository.GetAllAsync(
            cancellationToken);

        // Level 1: Provider and external identifier.

        if (!string.IsNullOrWhiteSpace(request.ExternalId))
        {
            var externalId = request.ExternalId.Trim();

            foreach (var job in jobs)
            {
                if (job.Sources.Any(source =>
                    source.SourceType == request.SourceType &&
                    string.Equals(
                        source.ExternalId,
                        externalId,
                        StringComparison.Ordinal)))
                {
                    return new DuplicateCheckResult(
                        DuplicateMatchType.ExternalId,
                        job.Id,
                        "Matching provider and external ID.");
                }
            }
        }

        // Level 2: URL fingerprint.

        foreach (var job in jobs)
        {
            if (job.Sources.Any(source =>
                source.UrlFingerprint == fingerprint &&
                source.NormalizedUrl == normalizedUrl))
            {
                return new DuplicateCheckResult(
                    DuplicateMatchType.NormalizedUrl,
                    job.Id,
                    "Matching normalized URL.");
            }
        }

        // Level 3: Conservative metadata comparison.

        var title = NormalizeText(request.Title);
        var company = NormalizeText(request.Company);
        var location = NormalizeText(request.Location);

        if (title.Length > 0 &&
            company.Length > 0 &&
            location.Length > 0)
        {
            foreach (var job in jobs)
            {
                if (NormalizeText(job.Title) == title &&
                    NormalizeText(job.Company) == company &&
                    NormalizeText(job.Location) == location)
                {
                    return new DuplicateCheckResult(
                        DuplicateMatchType.PossibleMatch,
                        job.Id,
                        "Matching title, company and location.");
                }
            }
        }

        return new DuplicateCheckResult(
            DuplicateMatchType.None,
            null,
            "No duplicate detected.");
    }

    public static string NormalizeUrl(string url)
    {
        return UrlNormalizer.Normalize(url);
    }

    public static string NormalizeText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        return Regex.Replace(
            text.Trim().ToLowerInvariant(),
            @"\s+",
            " ");
    }
}
