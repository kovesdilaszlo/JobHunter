
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using JobHunter.Application.DTOs;
using JobHunter.Application.Interfaces;
using JobHunter.Domain.Enums;
using Microsoft.Extensions.Options;

namespace JobHunter.Infrastructure.Providers.Jooble;

public sealed class JoobleJobProvider : IJobProvider
{
    private readonly HttpClient _httpClient;

    private readonly JoobleOptions _options;

    public JoobleJobProvider(
        HttpClient httpClient,
        IOptions<JoobleOptions> options)
    {
        _httpClient = httpClient
            ?? throw new ArgumentNullException(
                nameof(httpClient));

        ArgumentNullException.ThrowIfNull(options);

        _options = options.Value;
    }

    public string Name => "Jooble";

    public JobSourceType SourceType =>
        JobSourceType.Jooble;

    public async Task<IReadOnlyList<ExternalJobDto>> SearchAsync(
        JobProviderSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateRequest(request);

        ValidateOptions();

        cancellationToken.ThrowIfCancellationRequested();

        var results = new List<ExternalJobDto>();

        for (var page = 1;
             page <= _options.MaxPages;
             page++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var remaining =
                request.MaxResults - results.Count;

            if (remaining <= 0)
            {
                break;
            }

            var pageSize = Math.Min(
                _options.PageSize,
                remaining);

            var apiRequest = new JoobleSearchRequest
            {
                Keywords = request.Keywords.Trim(),
                Location = request.Location.Trim(),

                Radius = _options.Radius.ToString(
                    CultureInfo.InvariantCulture),

                Page = page,
                ResultOnPage = pageSize,
                CompanySearch = false
            };

            var response = await RequestPageAsync(
                apiRequest,
                cancellationToken);

            if (response.Jobs is null ||
                response.Jobs.Count == 0)
            {
                break;
            }

            foreach (var job in response.Jobs)
            {
                cancellationToken.ThrowIfCancellationRequested();

                results.Add(MapToExternalJob(job));

                if (results.Count >= request.MaxResults)
                {
                    break;
                }
            }

            if (results.Count >= request.MaxResults)
            {
                break;
            }

            if (results.Count >= response.TotalCount)
            {
                break;
            }

            if (response.Jobs.Count < pageSize)
            {
                break;
            }
        }

        return results.AsReadOnly();
    }

    private async Task<JoobleSearchResponse> RequestPageAsync(
        JoobleSearchRequest request,
        CancellationToken cancellationToken)
    {
        var endpoint =
            $"api/{Uri.EscapeDataString(_options.ApiKey)}";

        try
        {
            using var response =
                await _httpClient.PostAsJsonAsync(
                    endpoint,
                    request,
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw CreateApiException(
                    response.StatusCode);
            }

            var result = await response.Content
                .ReadFromJsonAsync<JoobleSearchResponse>(
                    cancellationToken: cancellationToken);

            if (result is null || result.Jobs is null)
            {
                throw new JoobleApiException(
                    "Jooble returned an invalid response.");
            }

            return result;
        }
        catch (HttpRequestException exception)
        {
            // Do not include the request URI or API key.
            throw new JoobleApiException(
                "Communication with Jooble failed.",
                exception.StatusCode);
        }
        catch (JsonException)
        {
            throw new JoobleApiException(
                "Jooble returned invalid JSON.");
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            throw new JoobleApiException(
                "The Jooble API request timed out.");
        }
    }

    private static ExternalJobDto MapToExternalJob(
        JoobleJobResponse job)
    {
        return new ExternalJobDto(
            Title: job.Title ?? string.Empty,

            Company: job.Company ?? string.Empty,

            Location: job.Location ?? string.Empty,

            Description: job.Snippet ?? string.Empty,

            SourceUrl: job.Link ?? string.Empty,

            ExternalId: job.Id?.ToString(
                CultureInfo.InvariantCulture),

            WorkArrangement: WorkArrangement.Unknown);
    }

    private static JoobleApiException CreateApiException(
        HttpStatusCode statusCode)
    {
        var message = statusCode switch
        {
            HttpStatusCode.Forbidden =>
                "Jooble API access denied. Check the API key.",

            HttpStatusCode.Unauthorized =>
                "Jooble API authentication failed.",

            HttpStatusCode.TooManyRequests =>
                "Jooble API request limit exceeded.",

            HttpStatusCode.NotFound =>
                "Jooble API endpoint was not found.",

            _ =>
                $"Jooble API returned HTTP {(int)statusCode}."
        };

        return new JoobleApiException(
            message,
            statusCode);
    }

    private static void ValidateRequest(
        JobProviderSearchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Keywords))
        {
            throw new ArgumentException(
                "Keywords cannot be empty.",
                nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Location))
        {
            throw new ArgumentException(
                "Location cannot be empty.",
                nameof(request));
        }

        if (request.MaxResults <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                "MaxResults must be greater than zero.");
        }
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException(
                "The Jooble API key is not configured.");
        }

        if (_options.PageSize <= 0)
        {
            throw new InvalidOperationException(
                "Jooble PageSize must be greater than zero.");
        }

        if (_options.MaxPages <= 0)
        {
            throw new InvalidOperationException(
                "Jooble MaxPages must be greater than zero.");
        }

        if (_options.Radius is not
            (0 or 4 or 8 or 16 or 26 or 40 or 80))
        {
            throw new InvalidOperationException(
                "Invalid Jooble search radius.");
        }

        if (_options.TimeoutSeconds <= 0)
        {
            throw new InvalidOperationException(
                "Jooble timeout must be greater than zero.");
        }
    }
}
