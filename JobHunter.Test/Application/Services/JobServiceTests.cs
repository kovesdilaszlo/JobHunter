
using JobHunter.Application.DTOs;
using JobHunter.Application.Enums;
using JobHunter.Application.Interfaces;
using JobHunter.Application.Services;
using JobHunter.Domain.Entities;
using JobHunter.Domain.Enums;

namespace JobHunter.Test.Application.Services;

public class JobServiceTests
{
    private static CreateJobRequest CreateRequest(
        string url = "https://example.com/jobs/123",
        string? externalId = "123")
    {
        return new CreateJobRequest(
            "Junior .NET Developer",
            "Example Company",
            "Budapest",
            "Junior developer position",
            WorkArrangement.Hybrid,
            JobSourceType.Jooble,
            url,
            externalId);
    }

    private static JobService CreateService(
        FakeJobRepository repository)
    {
        var duplicateService =
            new DuplicateDetectionService(repository);

        return new JobService(repository, duplicateService);
    }

    [Fact]
    public void Constructor_NullRepository_ThrowsException()
    {
        var repository = new FakeJobRepository();

        Assert.Throws<ArgumentNullException>(() =>
            new JobService(
                null!,
                new DuplicateDetectionService(repository)));
    }

    [Fact]
    public void Constructor_NullDuplicateService_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new JobService(
                new FakeJobRepository(),
                null!));
    }

    [Fact]
    public async Task CreateAsync_NewJob_CreatesJob()
    {
        var repository = new FakeJobRepository();
        var service = CreateService(repository);

        var result = await service.CreateAsync(CreateRequest());

        Assert.True(result.WasCreated);
        Assert.True(result.SourceWasAdded);
        Assert.Equal(DuplicateMatchType.None, result.MatchType);
        Assert.Single(repository.Jobs);
        Assert.Single(result.Job.Sources);
        Assert.Equal(1, repository.SaveChangesCount);
    }

    [Fact]
    public async Task CreateAsync_SameExternalId_DoesNotCreateNewJob()
    {
        var repository = new FakeJobRepository();
        var service = CreateService(repository);

        var first = await service.CreateAsync(CreateRequest());

        var second = await service.CreateAsync(
            CreateRequest(
                "https://other.com/jobs/456",
                "123"));

        Assert.False(second.WasCreated);
        Assert.False(second.SourceWasAdded);

        Assert.Equal(
            DuplicateMatchType.ExternalId,
            second.MatchType);

        Assert.Equal(first.Job.Id, second.Job.Id);

        Assert.Single(repository.Jobs);
        Assert.Single(second.Job.Sources);

        Assert.Equal(1, repository.SaveChangesCount);
    }


    [Fact]
    public async Task CreateAsync_SameUrl_DoesNotCreateNewJob()
    {
        var repository = new FakeJobRepository();
        var service = CreateService(repository);

        await service.CreateAsync(CreateRequest());

        var second = await service.CreateAsync(
            CreateRequest(
                "https://example.com/jobs/123",
                null));

        Assert.False(second.WasCreated);
        Assert.False(second.SourceWasAdded);
        Assert.Equal(
            DuplicateMatchType.NormalizedUrl,
            second.MatchType);
        Assert.Single(repository.Jobs);
    }

    [Fact]
    public async Task CreateAsync_TrackingUrl_DoesNotCreateNewJob()
    {
        var repository = new FakeJobRepository();
        var service = CreateService(repository);

        await service.CreateAsync(CreateRequest());

        var second = await service.CreateAsync(
            CreateRequest(
                "https://example.com/jobs/123?utm_source=google",
                null));

        Assert.False(second.WasCreated);
        Assert.Single(repository.Jobs);
    }

    [Fact]
    public async Task CreateAsync_PossibleMatch_CreatesSeparateJob()
    {
        var repository = new FakeJobRepository();
        var service = CreateService(repository);

        var first = await service.CreateAsync(CreateRequest());

        var second = await service.CreateAsync(
            CreateRequest(
                "https://other.com/jobs/999",
                "999"));

        Assert.True(second.WasCreated);
        Assert.Equal(
            DuplicateMatchType.PossibleMatch,
            second.MatchType);
        Assert.Equal(
            first.Job.Id,
            second.PossibleDuplicateJobId);
        Assert.Equal(2, repository.Jobs.Count);
    }

    [Fact]
    public async Task CreateAsync_InvalidTitle_ThrowsException()
    {
        var service = CreateService(
            new FakeJobRepository());

        var request = CreateRequest() with { Title = "" };

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_NullRequest_ThrowsException()
    {
        var service = CreateService(
            new FakeJobRepository());

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.CreateAsync(null!));
    }

    [Fact]
    public async Task GetByIdAsync_ExistingJob_ReturnsJob()
    {
        var service = CreateService(
            new FakeJobRepository());

        var created = await service.CreateAsync(
            CreateRequest());

        var result = await service.GetByIdAsync(
            created.Job.Id);

        Assert.NotNull(result);
        Assert.Equal(created.Job.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownJob_ReturnsNull()
    {
        var service = CreateService(
            new FakeJobRepository());

        var result = await service.GetByIdAsync(
            Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_MultipleJobs_ReturnsAll()
    {
        var service = CreateService(
            new FakeJobRepository());

        await service.CreateAsync(CreateRequest());

        await service.CreateAsync(
            CreateRequest(
                "https://example.com/jobs/456",
                "456") with
            {
                Title = "SQL Developer"
            });

        var results = await service.GetAllAsync();

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task ChangeStatusAsync_ExistingJob_UpdatesStatus()
    {
        var repository = new FakeJobRepository();
        var service = CreateService(repository);

        var created = await service.CreateAsync(
            CreateRequest());

        var updated = await service.ChangeStatusAsync(
            created.Job.Id,
            ApplicationStatus.Applied);

        var result = await service.GetByIdAsync(
            created.Job.Id);

        Assert.True(updated);
        Assert.NotNull(result);
        Assert.Equal(ApplicationStatus.Applied, result.Status);
    }

    [Fact]
    public async Task ArchiveAsync_ExistingJob_ArchivesJob()
    {
        var service = CreateService(
            new FakeJobRepository());

        var created = await service.CreateAsync(
            CreateRequest());

        var archived = await service.ArchiveAsync(
            created.Job.Id);

        var result = await service.GetByIdAsync(
            created.Job.Id);

        Assert.True(archived);
        Assert.NotNull(result);
        Assert.True(result.IsArchived);
    }

    [Fact]
    public async Task RestoreAsync_ArchivedJob_RestoresJob()
    {
        var service = CreateService(
            new FakeJobRepository());

        var created = await service.CreateAsync(
            CreateRequest());

        await service.ArchiveAsync(created.Job.Id);

        var restored = await service.RestoreAsync(
            created.Job.Id);

        var result = await service.GetByIdAsync(
            created.Job.Id);

        Assert.True(restored);
        Assert.NotNull(result);
        Assert.False(result.IsArchived);
    }

    private sealed class FakeJobRepository : IJobRepository
    {
        public List<JobPosting> Jobs { get; } = new();

        public int SaveChangesCount { get; private set; }

        public Task<JobPosting?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Jobs.FirstOrDefault(job => job.Id == id));
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
            SaveChangesCount++;
            return Task.CompletedTask;
        }
    }
}
