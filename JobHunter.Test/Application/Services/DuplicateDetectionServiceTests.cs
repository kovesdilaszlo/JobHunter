
using JobHunter.Application.DTOs;
using JobHunter.Application.Enums;
using JobHunter.Application.Interfaces;
using JobHunter.Application.Services;
using JobHunter.Domain.Entities;
using JobHunter.Domain.Enums;

namespace JobHunter.Test.Application.Services;

public class DuplicateDetectionServiceTests
{
    private static DuplicateCheckRequest CreateRequest(
        string url = "https://example.com/jobs/123",
        string? externalId = "123",
        JobSourceType sourceType = JobSourceType.Jooble)
    {
        return new DuplicateCheckRequest(
            sourceType,
            externalId,
            url,
            "Junior .NET Developer",
            "Example Company",
            "Budapest");
    }

    private static JobPosting CreateJob(
        string url = "https://example.com/jobs/123",
        string? externalId = "123",
        JobSourceType sourceType = JobSourceType.Jooble)
    {
        var job = new JobPosting(
            "Junior .NET Developer",
            "Example Company",
            "Budapest",
            "Developer position",
            WorkArrangement.Hybrid);

        job.AddSource(new JobPostingSource(
            sourceType, url, externalId));

        return job;
    }

    [Fact]
    public void Constructor_NullRepository_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new DuplicateDetectionService(null!));
    }

    [Fact]
    public async Task CheckAsync_NullRequest_ThrowsException()
    {
        var service = new DuplicateDetectionService(
            new FakeJobRepository());

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.CheckAsync(null!));
    }

    [Fact]
    public async Task CheckAsync_EmptyRepository_ReturnsNone()
    {
        var service = new DuplicateDetectionService(
            new FakeJobRepository());

        var result = await service.CheckAsync(CreateRequest());

        Assert.Equal(DuplicateMatchType.None, result.MatchType);
        Assert.Null(result.ExistingJobId);
        Assert.False(result.IsConfirmedDuplicate);
    }

    [Fact]
    public async Task CheckAsync_SameProviderAndExternalId_ReturnsMatch()
    {
        var repository = new FakeJobRepository();
        var existing = CreateJob();

        repository.Jobs.Add(existing);

        var service = new DuplicateDetectionService(repository);

        var result = await service.CheckAsync(
            CreateRequest(
                "https://other.com/job/456",
                "123"));

        Assert.Equal(
            DuplicateMatchType.ExternalId,
            result.MatchType);
        Assert.Equal(existing.Id, result.ExistingJobId);
        Assert.True(result.IsConfirmedDuplicate);
    }

    [Fact]
    public async Task CheckAsync_DifferentProviderSameExternalId_DoesNotMatchId()
    {
        var repository = new FakeJobRepository();
        repository.Jobs.Add(CreateJob());

        var service = new DuplicateDetectionService(repository);

        var result = await service.CheckAsync(
            CreateRequest(
                "https://other.com/jobs/456",
                "123",
                JobSourceType.LinkedIn));

        Assert.Equal(
            DuplicateMatchType.PossibleMatch,
            result.MatchType);
        Assert.False(result.IsConfirmedDuplicate);
    }

    [Fact]
    public async Task CheckAsync_SameUrl_ReturnsNormalizedUrlMatch()
    {
        var repository = new FakeJobRepository();
        var existing = CreateJob();

        repository.Jobs.Add(existing);

        var service = new DuplicateDetectionService(repository);

        var result = await service.CheckAsync(
            CreateRequest(
                "https://example.com/jobs/123",
                null));

        Assert.Equal(
            DuplicateMatchType.NormalizedUrl,
            result.MatchType);
        Assert.Equal(existing.Id, result.ExistingJobId);
    }

    [Fact]
    public async Task CheckAsync_TrackingParameters_Ignored()
    {
        var repository = new FakeJobRepository();
        repository.Jobs.Add(CreateJob());

        var service = new DuplicateDetectionService(repository);

        var result = await service.CheckAsync(
            CreateRequest(
                "https://example.com/jobs/123?utm_source=google",
                null));

        Assert.Equal(
            DuplicateMatchType.NormalizedUrl,
            result.MatchType);
    }

    [Fact]
    public async Task CheckAsync_SameMetadata_ReturnsPossibleMatch()
    {
        var repository = new FakeJobRepository();
        var existing = CreateJob();

        repository.Jobs.Add(existing);

        var service = new DuplicateDetectionService(repository);

        var request = CreateRequest(
            "https://other.com/jobs/999",
            "different-id");

        var result = await service.CheckAsync(request);

        Assert.Equal(
            DuplicateMatchType.PossibleMatch,
            result.MatchType);
        Assert.Equal(existing.Id, result.ExistingJobId);
        Assert.True(result.IsPossibleDuplicate);
    }

    [Fact]
    public async Task CheckAsync_DifferentCompany_ReturnsNone()
    {
        var repository = new FakeJobRepository();
        repository.Jobs.Add(CreateJob());

        var service = new DuplicateDetectionService(repository);

        var request = CreateRequest(
            "https://other.com/jobs/999",
            "different-id") with
        {
            Company = "Another Company"
        };

        var result = await service.CheckAsync(request);

        Assert.Equal(DuplicateMatchType.None, result.MatchType);
    }

    [Fact]
    public async Task CheckAsync_InvalidUrl_ThrowsException()
    {
        var service = new DuplicateDetectionService(
            new FakeJobRepository());

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CheckAsync(
                CreateRequest("not-a-url")));
    }

    [Fact]
    public async Task CheckAsync_InvalidSourceType_ThrowsException()
    {
        var service = new DuplicateDetectionService(
            new FakeJobRepository());

        var request = CreateRequest() with
        {
            SourceType = (JobSourceType)999
        };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.CheckAsync(request));
    }


    [Theory]
    [InlineData(
        "https://example.com/job/123?utm_source=google",
        "https://example.com/job/123")]
    [InlineData(
        "https://example.com/job/123#details",
        "https://example.com/job/123")]
    [InlineData(
        "https://example.com/job/123/",
        "https://example.com/job/123")]
    [InlineData(
        "https://example.com/job/123?b=2&a=1",
        "https://example.com/job/123?b=2&a=1")]
    public void NormalizeUrl_ValidUrl_ReturnsExpected(
        string input,
        string expected)
    {
        var result = DuplicateDetectionService.NormalizeUrl(input);

        Assert.Equal(expected, result);
    }


    [Fact]
    public void DuplicateCheckResult_ExternalId_IsConfirmed()
    {
        var result = new DuplicateCheckResult(
            DuplicateMatchType.ExternalId,
            Guid.NewGuid(),
            "External ID match");

        Assert.True(result.IsConfirmedDuplicate);
        Assert.False(result.IsPossibleDuplicate);
    }

    [Fact]
    public void DuplicateCheckResult_PossibleMatch_IsNotConfirmed()
    {
        var result = new DuplicateCheckResult(
            DuplicateMatchType.PossibleMatch,
            Guid.NewGuid(),
            "Metadata match");

        Assert.False(result.IsConfirmedDuplicate);
        Assert.True(result.IsPossibleDuplicate);
    }

    private sealed class FakeJobRepository : IJobRepository
    {
        public List<JobPosting> Jobs { get; } = new();

        public Task<JobPosting?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Jobs.FirstOrDefault(j => j.Id == id));
        }

        public Task<IReadOnlyList<JobPosting>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<JobPosting>>(
                Jobs.ToList());
        }

        public Task AddAsync(
            JobPosting job,
            CancellationToken cancellationToken = default)
        {
            Jobs.Add(job);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
