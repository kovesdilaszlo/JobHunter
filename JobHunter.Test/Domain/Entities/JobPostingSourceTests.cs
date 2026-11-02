
using JobHunter.Domain.Entities;
using JobHunter.Domain.Enums;

namespace JobHunter.Test.Domain.Entities;

public class JobPostingSourceTests
{
    [Fact]
    public void Constructor_ValidData_CreatesSource()
    {
        var source = new JobPostingSource(
            JobSourceType.Jooble,
            "https://example.com/jobs/123",
            "external-123");

        Assert.NotEqual(Guid.Empty, source.Id);
        Assert.Equal(JobSourceType.Jooble, source.SourceType);
        Assert.Equal("external-123", source.ExternalId);
        Assert.Equal(
            "https://example.com/jobs/123",
            source.NormalizedUrl);
        Assert.Equal(Guid.Empty, source.JobPostingId);
    }

    [Fact]
    public void Constructor_InvalidSourceType_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new JobPostingSource(
                (JobSourceType)999,
                "https://example.com/jobs/123"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com/job")]
    [InlineData("javascript:alert(1)")]
    public void Constructor_InvalidUrl_ThrowsArgumentException(
        string url)
    {
        Assert.Throws<ArgumentException>(() =>
            new JobPostingSource(
                JobSourceType.Manual,
                url));
    }

    [Fact]
    public void Constructor_HttpUrl_AcceptsUrl()
    {
        var source = new JobPostingSource(
            JobSourceType.Manual,
            "http://example.com/job");

        Assert.StartsWith("http://", source.OriginalUrl);
    }

    [Fact]
    public void Constructor_UrlWithFragment_RemovesFragment()
    {
        var source = new JobPostingSource(
            JobSourceType.LinkedIn,
            "https://example.com/jobs/123#details");

        Assert.Equal(
            "https://example.com/jobs/123",
            source.NormalizedUrl);
    }

    [Fact]
    public void Constructor_TrailingSlash_RemovesTrailingSlash()
    {
        var source = new JobPostingSource(
            JobSourceType.Jooble,
            "https://example.com/jobs/123/");

        Assert.Equal(
            "https://example.com/jobs/123",
            source.NormalizedUrl);
    }

    [Fact]
    public void Constructor_WhitespaceExternalId_UsesNull()
    {
        var source = new JobPostingSource(
            JobSourceType.Jooble,
            "https://example.com/jobs/123",
            "   ");

        Assert.Null(source.ExternalId);
    }

    [Fact]
    public void Constructor_ExternalId_TrimsWhitespace()
    {
        var source = new JobPostingSource(
            JobSourceType.Jooble,
            "https://example.com/jobs/123",
            "  abc-123  ");

        Assert.Equal("abc-123", source.ExternalId);
    }

    [Fact]
    public void MarkAsSeen_UpdatesLastSeenAt()
    {
        var source = new JobPostingSource(
            JobSourceType.Jooble,
            "https://example.com/jobs/123");

        Assert.Null(source.LastSeenAt);

        var before = DateTimeOffset.UtcNow;

        source.MarkAsSeen();

        var after = DateTimeOffset.UtcNow;

        Assert.NotNull(source.LastSeenAt);
        Assert.InRange(source.LastSeenAt.Value, before, after);
    }
}
