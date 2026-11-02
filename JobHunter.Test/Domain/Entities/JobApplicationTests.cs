
using JobHunter.Domain.Entities;

namespace JobHunter.Test.Domain.Entities;

public class JobApplicationTests
{
    private static readonly Guid ValidJobId = Guid.NewGuid();

    [Fact]
    public void Constructor_ValidJobId_CreatesApplication()
    {
        var application = new JobApplication(ValidJobId);

        Assert.NotEqual(Guid.Empty, application.Id);
        Assert.Equal(ValidJobId, application.JobPostingId);
        Assert.Null(application.AppliedAt);
        Assert.Null(application.Notes);
    }

    [Fact]
    public void Constructor_EmptyJobId_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            new JobApplication(Guid.Empty));
    }

    [Fact]
    public void Constructor_Notes_TrimsWhitespace()
    {
        var application = new JobApplication(
            ValidJobId,
            "  Sent CV  ");

        Assert.Equal("Sent CV", application.Notes);
    }

    [Fact]
    public void Constructor_EmptyNotes_UsesNull()
    {
        var application = new JobApplication(
            ValidJobId,
            "   ");

        Assert.Null(application.Notes);
    }

    [Fact]
    public void MarkAsApplied_ValidDate_SetsAppliedAt()
    {
        var application = new JobApplication(ValidJobId);
        var date = DateTimeOffset.UtcNow.AddDays(-1);

        application.MarkAsApplied(date);

        Assert.Equal(date, application.AppliedAt);
    }

    [Fact]
    public void MarkAsApplied_FutureDate_ThrowsException()
    {
        var application = new JobApplication(ValidJobId);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            application.MarkAsApplied(
                DateTimeOffset.UtcNow.AddDays(1)));

        Assert.Null(application.AppliedAt);
    }

    [Fact]
    public void MarkAsApplied_DefaultDate_ThrowsException()
    {
        var application = new JobApplication(ValidJobId);

        Assert.Throws<ArgumentException>(() =>
            application.MarkAsApplied(default));
    }

    [Fact]
    public void UpdateNotes_ValidText_UpdatesNotes()
    {
        var application = new JobApplication(ValidJobId);

        application.UpdateNotes("  Interview scheduled  ");

        Assert.Equal(
            "Interview scheduled",
            application.Notes);
    }

    [Fact]
    public void UpdateNotes_EmptyText_ClearsNotes()
    {
        var application = new JobApplication(
            ValidJobId,
            "Previous note");

        application.UpdateNotes(" ");

        Assert.Null(application.Notes);
    }

    [Fact]
    public void UpdateNotes_UpdatesTimestamp()
    {
        var application = new JobApplication(ValidJobId);

        var before = DateTimeOffset.UtcNow;

        application.UpdateNotes("Updated");

        var after = DateTimeOffset.UtcNow;

        Assert.InRange(
            application.LastUpdatedAt,
            before,
            after);
    }
}
