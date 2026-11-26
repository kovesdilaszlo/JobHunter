
using JobHunter.Application.DTOs;
using JobHunter.Application.Enums;
using JobHunter.Application.Interfaces;
using JobHunter.Application.Services;
using JobHunter.Domain.Entities;
using JobHunter.Domain.Enums;
using Xunit;

namespace JobHunter.Test.Application.Services;

public class JobImportServiceTests
{
    private static JobProviderSearchRequest CreateRequest()
    {
        return new JobProviderSearchRequest(
            "Junior .NET Developer",
            "Budapest",
            true,
            100);
    }

    private static ExternalJobDto CreateExternalJob(
        string externalId = "123",
        string title = "Junior .NET Developer",
        string? url = null)
    {
        return new ExternalJobDto(
            title,
            "Example Company",
            "Budapest",
            "Developer position",
            url ?? $"https://example.com/jobs/{externalId}",
            externalId,
            WorkArrangement.Hybrid);
    }

    private static JobImportService CreateService(
        FakeJobRepository repository,
        params IJobProvider[] providers)
    {
        var duplicateService =
            new DuplicateDetectionService(repository);

        var jobService =
            new JobService(repository, duplicateService);

        return new JobImportService(
            jobService,
            providers);
    }

    [Fact]
    public async Task ImportAsync_ValidJob_InsertsJob()
    {
        var repository = new FakeJobRepository();

        var provider = new FakeJobProvider(
            CreateExternalJob());

        var service = CreateService(
            repository,
            provider);

        var result = await service.ImportAsync(
            JobSourceType.Jooble,
            CreateRequest());

        Assert.Equal(1, result.RetrievedCount);
        Assert.Equal(1, result.InsertedCount);
        Assert.Equal(0, result.DuplicateCount);
        Assert.Equal(0, result.FailedCount);

        Assert.True(result.IsSuccessful);

        Assert.Single(repository.Jobs);
    }

    [Fact]
    public async Task ImportAsync_MultipleJobs_InsertsAll()
    {
        var repository = new FakeJobRepository();

        var provider = new FakeJobProvider(
            CreateExternalJob("1", "Developer"),
            CreateExternalJob("2", "Data Analyst"),
            CreateExternalJob("3", "BI Analyst"));

        var service = CreateService(
            repository,
            provider);

        var result = await service.ImportAsync(
            JobSourceType.Jooble,
            CreateRequest());

        Assert.Equal(3, result.RetrievedCount);
        Assert.Equal(3, result.InsertedCount);
        Assert.Equal(3, repository.Jobs.Count);
    }

    [Fact]
    public async Task ImportAsync_DuplicateJob_DoesNotInsertTwice()
    {
        var repository = new FakeJobRepository();

        var provider = new FakeJobProvider(
            CreateExternalJob("123"),
            CreateExternalJob("123"));

        var service = CreateService(
            repository,
            provider);

        var result = await service.ImportAsync(
            JobSourceType.Jooble,
            CreateRequest());

        Assert.Equal(2, result.RetrievedCount);
        Assert.Equal(1, result.InsertedCount);
        Assert.Equal(1, result.DuplicateCount);

        Assert.Single(repository.Jobs);
    }

    [Fact]
    public async Task ImportAsync_RepeatedImport_DetectsDuplicate()
    {
        var repository = new FakeJobRepository();

        var provider = new FakeJobProvider(
            CreateExternalJob());

        var service = CreateService(
            repository,
            provider);

        var first = await service.ImportAsync(
            JobSourceType.Jooble,
            CreateRequest());

        var second = await service.ImportAsync(
            JobSourceType.Jooble,
            CreateRequest());

        Assert.Equal(1, first.InsertedCount);

        Assert.Equal(0, second.InsertedCount);
        Assert.Equal(1, second.DuplicateCount);

        Assert.Single(repository.Jobs);
    }

    [Fact]
    public async Task ImportAsync_PossibleDuplicate_InsertsSeparateJob()
    {
        var repository = new FakeJobRepository();

        var provider = new FakeJobProvider(
            CreateExternalJob("123"),
            CreateExternalJob("456"));

        var service = CreateService(
            repository,
            provider);

        var result = await service.ImportAsync(
            JobSourceType.Jooble,
            CreateRequest());

        Assert.Equal(2, result.InsertedCount);
        Assert.Equal(1, result.PossibleDuplicateCount);
        Assert.Equal(0, result.DuplicateCount);

        Assert.Equal(2, repository.Jobs.Count);
    }

    [Fact]
    public async Task ImportAsync_EmptyProvider_ReturnsZeroCounts()
    {
        var repository = new FakeJobRepository();

        var service = CreateService(
            repository,
            new FakeJobProvider());

        var result = await service.ImportAsync(
            JobSourceType.Jooble,
            CreateRequest());

        Assert.Equal(0, result.RetrievedCount);
        Assert.Equal(0, result.InsertedCount);
        Assert.Equal(0, result.DuplicateCount);
        Assert.Equal(0, result.FailedCount);

        Assert.True(result.IsSuccessful);
        Assert.Empty(repository.Jobs);
    }

    [Fact]
    public async Task ImportAsync_InvalidJob_RecordsError()
    {
        var repository = new FakeJobRepository();

        var provider = new FakeJobProvider(
            CreateExternalJob(title: ""));

        var service = CreateService(
            repository,
            provider);

        var result = await service.ImportAsync(
            JobSourceType.Jooble,
            CreateRequest());

        Assert.Equal(1, result.FailedCount);
        Assert.Equal(0, result.InsertedCount);

        Assert.False(result.IsSuccessful);

        Assert.Single(result.Errors);
        Assert.Empty(repository.Jobs);
    }

    [Fact]
    public async Task ImportAsync_InvalidUrl_RecordsError()
    {
        var repository = new FakeJobRepository();

        var provider = new FakeJobProvider(
            CreateExternalJob(url: "invalid-url"));

        var service = CreateService(
            repository,
            provider);

        var result = await service.ImportAsync(
            JobSourceType.Jooble,
            CreateRequest());

        Assert.Equal(1, result.FailedCount);
        Assert.Single(result.Errors);
        Assert.Empty(repository.Jobs);
    }

    [Fact]
    public async Task ImportAsync_MixedJobs_ContinuesAfterValidationError()
    {
        var repository = new FakeJobRepository();

        var provider = new FakeJobProvider(
            CreateExternalJob("1", ""),
            CreateExternalJob("2", "Developer"));

        var service = CreateService(
            repository,
            provider);

        var result = await service.ImportAsync(
            JobSourceType.Jooble,
            CreateRequest());

        Assert.Equal(2, result.RetrievedCount);
        Assert.Equal(1, result.InsertedCount);
        Assert.Equal(1, result.FailedCount);

        Assert.False(result.IsSuccessful);
        Assert.True(result.IsPartiallySuccessful);

        Assert.Single(repository.Jobs);
    }

    [Fact]
    public async Task ImportAsync_MaxResults_LimitsImport()
    {
        var repository = new FakeJobRepository();

        var provider = new FakeJobProvider(
            CreateExternalJob("1", "Developer"),
            CreateExternalJob("2", "Data Analyst"),
            CreateExternalJob("3", "BI Analyst"));

        var service = CreateService(
            repository,
            provider);

        var request = CreateRequest() with
        {
            MaxResults = 2
        };

        var result = await service.ImportAsync(
            JobSourceType.Jooble,
            request);

        Assert.Equal(2, result.RetrievedCount);
        Assert.Equal(2, result.InsertedCount);

        Assert.Equal(2, repository.Jobs.Count);
    }

    [Fact]
    public async Task ImportAsync_UnknownProvider_ThrowsException()
    {
        var service = CreateService(
            new FakeJobRepository());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ImportAsync(
                JobSourceType.Jooble,
                CreateRequest()));
    }

    [Fact]
    public async Task ImportAsync_NullRequest_ThrowsException()
    {
        var service = CreateService(
            new FakeJobRepository(),
            new FakeJobProvider());

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.ImportAsync(
                JobSourceType.Jooble,
                null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ImportAsync_InvalidKeywords_ThrowsException(
        string keywords)
    {
        var service = CreateService(
            new FakeJobRepository(),
            new FakeJobProvider());

        var request = CreateRequest() with
        {
            Keywords = keywords
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ImportAsync(
                JobSourceType.Jooble,
                request));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ImportAsync_InvalidLocation_ThrowsException(
        string location)
    {
        var service = CreateService(
            new FakeJobRepository(),
            new FakeJobProvider());

        var request = CreateRequest() with
        {
            Location = location
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ImportAsync(
                JobSourceType.Jooble,
                request));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ImportAsync_InvalidMaxResults_ThrowsException(
        int maxResults)
    {
        var service = CreateService(
            new FakeJobRepository(),
            new FakeJobProvider());

        var request = CreateRequest() with
        {
            MaxResults = maxResults
        };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ImportAsync(
                JobSourceType.Jooble,
                request));
    }

    [Fact]
    public async Task ImportAsync_CancelledToken_ThrowsException()
    {
        var service = CreateService(
            new FakeJobRepository(),
            new FakeJobProvider());

        using var cancellation =
            new CancellationTokenSource();

        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(
                JobSourceType.Jooble,
                CreateRequest(),
                cancellation.Token));
    }

    [Fact]
    public async Task ImportAsync_ReturnsValidTimestamps()
    {
        var service = CreateService(
            new FakeJobRepository(),
            new FakeJobProvider());

        var before = DateTimeOffset.UtcNow;

        var result = await service.ImportAsync(
            JobSourceType.Jooble,
            CreateRequest());

        var after = DateTimeOffset.UtcNow;

        Assert.InRange(
            result.StartedAt,
            before,
            after);

        Assert.InRange(
            result.CompletedAt,
            result.StartedAt,
            after);

        Assert.True(result.Duration >= TimeSpan.Zero);
    }

    [Fact]
    public async Task ImportAsync_SameUrlDifferentProvider_AddsSource()
    {
        var repository = new FakeJobRepository();

        var job = CreateExternalJob();

        var jooble = new FakeJobProvider(
            JobSourceType.Jooble,
            job);

        var linkedIn = new FakeJobProvider(
            JobSourceType.LinkedIn,
            job);

        var service = CreateService(
            repository,
            jooble,
            linkedIn);

        await service.ImportAsync(
            JobSourceType.Jooble,
            CreateRequest());

        var result = await service.ImportAsync(
            JobSourceType.LinkedIn,
            CreateRequest());

        Assert.Equal(0, result.InsertedCount);
        Assert.Equal(1, result.DuplicateCount);
        Assert.Equal(1, result.AddedSourceCount);

        var storedJob = Assert.Single(repository.Jobs);

        Assert.Equal(2, storedJob.Sources.Count);
    }

    private sealed class FakeJobProvider : IJobProvider
    {
        private readonly IReadOnlyList<ExternalJobDto> _jobs;

        public FakeJobProvider(
            params ExternalJobDto[] jobs)
            : this(JobSourceType.Jooble, jobs)
        {
        }

        public FakeJobProvider(
            JobSourceType sourceType,
            params ExternalJobDto[] jobs)
        {
            SourceType = sourceType;
            _jobs = jobs;
        }

        public string Name => SourceType.ToString();

        public JobSourceType SourceType { get; }

        public Task<IReadOnlyList<ExternalJobDto>> SearchAsync(
            JobProviderSearchRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(_jobs);
        }
    }

    private sealed class FakeJobRepository : IJobRepository
    {
        public List<JobPosting> Jobs { get; } = new();

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
            return Task.CompletedTask;
        }
    }
}
