
using System.Text.RegularExpressions;
using JobHunter.Application.DTOs;
using JobHunter.Application.Enums;
using JobHunter.Application.Interfaces;
using JobHunter.Domain.Entities;

namespace JobHunter.Application.Services;

public sealed class DuplicateDetectionService
    : IDuplicateDetectionService
{
    private readonly IJobRepository _repository;

    private static readonly HashSet<string> TrackingParameters =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "fbclid",
            "gclid",
            "msclkid",
            "ref",
            "ref_src",
            "source"
        };

    public DuplicateDetectionService(IJobRepository repository)
    {
        _repository = repository
            ?? throw new ArgumentNullException(nameof(repository));
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

        var normalizedUrl = NormalizeUrl(request.SourceUrl);

        var jobs = await _repository.GetAllAsync(
            cancellationToken);

        // 1. Provider + external ID.
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

        // 2. Normalized URL.
        foreach (var job in jobs)
        {
            foreach (var source in job.Sources)
            {
                if (NormalizeUrl(source.OriginalUrl) == normalizedUrl)
                {
                    return new DuplicateCheckResult(
                        DuplicateMatchType.NormalizedUrl,
                        job.Id,
                        "Matching normalized URL.");
                }
            }
        }

        // 3. Conservative metadata comparison.
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
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp &&
             uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException(
                "A valid HTTP or HTTPS URL is required.",
                nameof(url));
        }

        var builder = new UriBuilder(uri)
        {
            Fragment = string.Empty,
            Query = string.Empty
        };

        var parameters = new List<KeyValuePair<string, string>>();

        var query = uri.Query.TrimStart('?');

        if (!string.IsNullOrEmpty(query))
        {
            foreach (var part in query.Split('&',
                StringSplitOptions.RemoveEmptyEntries))
            {
                var pieces = part.Split('=', 2);

                var key = Uri.UnescapeDataString(
                    pieces[0].Replace("+", " "));

                if (key.StartsWith(
                        "utm_",
                        StringComparison.OrdinalIgnoreCase) ||
                    TrackingParameters.Contains(key))
                {
                    continue;
                }

                var value = pieces.Length > 1
                    ? Uri.UnescapeDataString(
                        pieces[1].Replace("+", " "))
                    : string.Empty;

                parameters.Add(
                    new KeyValuePair<string, string>(key, value));
            }
        }

        if (parameters.Count > 0)
        {
            builder.Query = string.Join("&",
                parameters
                    .OrderBy(p => p.Key, StringComparer.Ordinal)
                    .ThenBy(p => p.Value, StringComparer.Ordinal)
                    .Select(p =>
                        $"{Uri.EscapeDataString(p.Key)}=" +
                        $"{Uri.EscapeDataString(p.Value)}"));
        }

        var result = builder.Uri.AbsoluteUri.TrimEnd('/');

        return result;
    }

    public static string NormalizeText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var normalized = text.Trim().ToLowerInvariant();

        normalized = Regex.Replace(
            normalized,
            @"\s+",
            " ");

        return normalized;
    }
}
