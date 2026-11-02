
using JobHunter.Domain.Enums;

namespace JobHunter.Domain.Entities;

public class JobPosting
{
    private readonly List<JobPostingSource> _sources = new();

    private JobPosting()
    {
        // Required by EF Core.
    }

    public JobPosting(
        string title,
        string company,
        string location,
        string description,
        WorkArrangement workArrangement)
    {
        Id = Guid.NewGuid();

        Title = RequireText(title, nameof(title));
        Company = RequireText(company, nameof(company));
        Location = RequireText(location, nameof(location));
        Description = description ?? string.Empty;

        if (!Enum.IsDefined(workArrangement))
            throw new ArgumentOutOfRangeException(nameof(workArrangement));

        WorkArrangement = workArrangement;
        Status = ApplicationStatus.New;
        DiscoveredAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Company { get; private set; } = string.Empty;

    public string Location { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public WorkArrangement WorkArrangement { get; private set; }

    public ApplicationStatus Status { get; private set; }

    public DateTimeOffset DiscoveredAt { get; private set; }

    public DateTimeOffset? PublishedAt { get; private set; }

    public bool IsArchived { get; private set; }

    public DateTimeOffset? ArchivedAt { get; private set; }

    public IReadOnlyCollection<JobPostingSource> Sources => _sources.AsReadOnly();

    public void ChangeStatus(ApplicationStatus newStatus)
    {
        if (!Enum.IsDefined(newStatus))
            throw new ArgumentOutOfRangeException(nameof(newStatus));

        Status = newStatus;
    }

    public void Archive()
    {
        if (IsArchived)
            return;

        IsArchived = true;
        ArchivedAt = DateTimeOffset.UtcNow;
    }

    public void Restore()
    {
        IsArchived = false;
        ArchivedAt = null;
    }

    public void AddSource(JobPostingSource source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (_sources.Any(s =>
            s.SourceType == source.SourceType &&
            s.NormalizedUrl == source.NormalizedUrl))
        {
            throw new InvalidOperationException(
                "This source already exists for the job posting.");
        }

        source.AssignToJob(Id);
        _sources.Add(source);
    }

    private static string RequireText(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(
                "Value cannot be empty.", parameterName);

        return value.Trim();
    }
}
