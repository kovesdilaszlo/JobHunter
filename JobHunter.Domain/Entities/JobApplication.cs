
using JobHunter.Domain.Enums;

namespace JobHunter.Domain.Entities;

public class JobApplication
{
    private JobApplication()
    {
        // Required by EF Core.
    }

    public JobApplication(Guid jobPostingId, string? notes = null)
    {
        if (jobPostingId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid job posting ID is required.",
                nameof(jobPostingId));
        }

        Id = Guid.NewGuid();
        JobPostingId = jobPostingId;
        Notes = NormalizeNotes(notes);
        LastUpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid JobPostingId { get; private set; }

    public DateTimeOffset? AppliedAt { get; private set; }

    public string? Notes { get; private set; }

    public DateTimeOffset LastUpdatedAt { get; private set; }

    public void MarkAsApplied(DateTimeOffset appliedAt)
    {
        if (appliedAt == default)
        {
            throw new ArgumentException(
                "A valid application date is required.",
                nameof(appliedAt));
        }

        if (appliedAt > DateTimeOffset.UtcNow)
        {
            throw new ArgumentOutOfRangeException(
                nameof(appliedAt),
                "Application date cannot be in the future.");
        }

        AppliedAt = appliedAt;
        LastUpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateNotes(string? notes)
    {
        Notes = NormalizeNotes(notes);
        LastUpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string? NormalizeNotes(string? notes)
    {
        return string.IsNullOrWhiteSpace(notes)
            ? null
            : notes.Trim();
    }
}
