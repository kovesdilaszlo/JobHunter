
using JobHunter.Domain.Entities;
using JobHunter.Domain.Enums;

namespace JobHunter.Test.Domain.Entities;

public class JobPostingTests
{
    private static JobPosting CreateValidJob()
    {
        return new JobPosting(
            "Junior C# Developer",
            "Example Company",
            "Budapest",
            "Junior software developer position.",
            WorkArrangement.Hybrid);
    }

    [Fact]
    public void Constructor_ValidData_CreatesJobPosting()
    {
        // Arrange & Act
        var job = CreateValidJob();

        // Assert
        Assert.NotEqual(Guid.Empty, job.Id);
        Assert.Equal("Junior C# Developer", job.Title);
        Assert.Equal("Example Company", job.Company);
        Assert.Equal("Budapest", job.Location);
        Assert.Equal(WorkArrangement.Hybrid, job.WorkArrangement);
        Assert.Equal(ApplicationStatus.New, job.Status);
        Assert.False(job.IsArchived);
        Assert.Empty(job.Sources);
    }

    [Fact]
    public void Constructor_ValidData_SetsDiscoveryTime()
    {
        var before = DateTimeOffset.UtcNow;

        var job = CreateValidJob();

        var after = DateTimeOffset.UtcNow;

        Assert.InRange(job.DiscoveredAt, before, after);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_EmptyTitle_ThrowsArgumentException(
        string title)
    {
        Assert.Throws<ArgumentException>(() =>
            new JobPosting(
                title,
                "Example Company",
                "Budapest",
                "Description",
                WorkArrangement.Hybrid));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_EmptyCompany_ThrowsArgumentException(
        string company)
    {
        Assert.Throws<ArgumentException>(() =>
            new JobPosting(
                "Junior Developer",
                company,
                "Budapest",
                "Description",
                WorkArrangement.Hybrid));
    }

    [Fact]
    public void Constructor_EmptyLocation_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new JobPosting(
                "Junior Developer",
                "Example Company",
                "",
                "Description",
                WorkArrangement.Remote));
    }

    [Fact]
    public void Constructor_InvalidWorkArrangement_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new JobPosting(
                "Junior Developer",
                "Example Company",
                "Budapest",
                "Description",
                (WorkArrangement)999));
    }

    [Fact]
    public void Constructor_WhitespaceValues_TrimsProperties()
    {
        var job = new JobPosting(
            "  Junior Developer  ",
            "  Example Company  ",
            "  Budapest  ",
            "Description",
            WorkArrangement.OnSite);

        Assert.Equal("Junior Developer", job.Title);
        Assert.Equal("Example Company", job.Company);
        Assert.Equal("Budapest", job.Location);
    }

    [Fact]
    public void Constructor_NullDescription_UsesEmptyString()
    {
        var job = new JobPosting(
            "Junior Developer",
            "Example Company",
            "Budapest",
            null!,
            WorkArrangement.Remote);

        Assert.Equal(string.Empty, job.Description);
    }

    [Fact]
    public void ChangeStatus_ValidStatus_UpdatesStatus()
    {
        var job = CreateValidJob();

        job.ChangeStatus(ApplicationStatus.Applied);

        Assert.Equal(ApplicationStatus.Applied, job.Status);
    }

    [Fact]
    public void ChangeStatus_InvalidStatus_ThrowsException()
    {
        var job = CreateValidJob();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            job.ChangeStatus((ApplicationStatus)999));

        Assert.Equal(ApplicationStatus.New, job.Status);
    }

    [Fact]
    public void Archive_ActiveJob_ArchivesJob()
    {
        var job = CreateValidJob();

        job.Archive();

        Assert.True(job.IsArchived);
        Assert.NotNull(job.ArchivedAt);
    }

    [Fact]
    public void Archive_AlreadyArchived_DoesNotChangeTimestamp()
    {
        var job = CreateValidJob();

        job.Archive();
        var originalTimestamp = job.ArchivedAt;

        job.Archive();

        Assert.Equal(originalTimestamp, job.ArchivedAt);
    }

    [Fact]
    public void Restore_ArchivedJob_RestoresJob()
    {
        var job = CreateValidJob();
        job.Archive();

        job.Restore();

        Assert.False(job.IsArchived);
        Assert.Null(job.ArchivedAt);
    }

    [Fact]
    public void AddSource_ValidSource_AddsSource()
    {
        var job = CreateValidJob();

        var source = new JobPostingSource(
            JobSourceType.Jooble,
            "https://example.com/jobs/123");

        job.AddSource(source);

        Assert.Single(job.Sources);
        Assert.Equal(job.Id, source.JobPostingId);
    }

    [Fact]
    public void AddSource_DuplicateSource_ThrowsException()
    {
        var job = CreateValidJob();

        job.AddSource(new JobPostingSource(
            JobSourceType.Jooble,
            "https://example.com/jobs/123"));

        var duplicate = new JobPostingSource(
            JobSourceType.Jooble,
            "https://example.com/jobs/123");

        Assert.Throws<InvalidOperationException>(() =>
            job.AddSource(duplicate));

        Assert.Single(job.Sources);
    }

    [Fact]
    public void AddSource_NullSource_ThrowsArgumentNullException()
    {
        var job = CreateValidJob();

        Assert.Throws<ArgumentNullException>(() =>
            job.AddSource(null!));
    }

    [Fact]
    public void AddSource_SameSourceDifferentJob_ThrowsException()
    {
        var firstJob = CreateValidJob();
        var secondJob = CreateValidJob();

        var source = new JobPostingSource(
            JobSourceType.Jooble,
            "https://example.com/jobs/123");

        firstJob.AddSource(source);

        Assert.Throws<InvalidOperationException>(() =>
            secondJob.AddSource(source));

        Assert.Empty(secondJob.Sources);
    }

    [Fact]
    public void AddSource_DifferentProviders_AllowsBoth()
    {
        var job = CreateValidJob();

        job.AddSource(new JobPostingSource(
            JobSourceType.Jooble,
            "https://example.com/jobs/123"));

        job.AddSource(new JobPostingSource(
            JobSourceType.LinkedIn,
            "https://linkedin.com/jobs/view/456"));

        Assert.Equal(2, job.Sources.Count);
    }
}
