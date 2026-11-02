
using JobHunter.Domain.Enums;

namespace JobHunter.Domain.Entities;

public class JobPostingSource
{
    private JobPostingSource()
    {
        // Required by EF Core.
    }

    public JobPostingSource(
        JobSourceType sourceType,
        string originalUrl,
        string? externalId = null)
    {
        if (!Enum.IsDefined(sourceType))
            throw new ArgumentOutOfRangeException(nameof(sourceType));

        if (!Uri.TryCreate(originalUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp &&
             uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException(
                "A valid HTTP or HTTPS URL is required.",
                nameof(originalUrl));
        }

        Id = Guid.NewGuid();
        SourceType = sourceType;
        OriginalUrl = uri.AbsoluteUri;
        NormalizedUrl = NormalizeUrl(uri);
        ExternalId = string.IsNullOrWhiteSpace(externalId)
            ? null
            : externalId.Trim();

        DiscoveredAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid JobPostingId { get; private set; }

    public JobSourceType SourceType { get; private set; }

    public string? ExternalId { get; private set; }

    public string OriginalUrl { get; private set; } = string.Empty;

    public string NormalizedUrl { get; private set; } = string.Empty;

    public DateTimeOffset DiscoveredAt { get; private set; }

    public DateTimeOffset? LastSeenAt { get; private set; }

    public void MarkAsSeen()
    {
        LastSeenAt = DateTimeOffset.UtcNow;
    }

    internal void AssignToJob(Guid jobPostingId)
    {
        if (jobPostingId == Guid.Empty)
            throw new ArgumentException(
                "A valid job identifier is required.",
                nameof(jobPostingId));

        if (JobPostingId != Guid.Empty &&
            JobPostingId != jobPostingId)
        {
            throw new InvalidOperationException(
                "Source already belongs to another job posting.");
        }

        JobPostingId = jobPostingId;
    }

    private static string NormalizeUrl(Uri uri)
    {
        var builder = new UriBuilder(uri)
        {
            Fragment = string.Empty
        };

        return builder.Uri.AbsoluteUri.TrimEnd('/');
    }
}
