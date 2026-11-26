
using JobHunter.Application.DTOs;
using JobHunter.Application.Interfaces;
using JobHunter.Application.Services;
using JobHunter.Domain.Entities;
using JobHunter.Domain.Enums;
using Xunit;

namespace JobHunter.Test.Application.Services;

public class SearchExecutionServiceTests
{
    private static JobSearchProfile CreateProfile()
    {
        return new JobSearchProfile(
            "Junior .NET Developer",
            "C#, ASP.NET Core",
            "Budapest",
            true);
    }

    private static JobImportResult CreateResult(
        int retrieved = 10,
        int inserted = 8,
        int duplicates = 2,
        int failed = 0)
    {
        var now = DateTimeOffset.UtcNow;

        return new JobImportResult(
            JobSourceType.Jooble,
            retrieved,
            inserted,
            duplicates,
            0,
            0,
            failed,
            now,
            now,
            Array.Empty<JobImportError>());
    }

    private static StartSearchRequest CreateRequest(
        JobSearchProfile profile)
    {
        return new StartSearchRequest(
            profile.Id,
            JobSourceType.Jooble,
            SearchTriggerType.Manual,
            100);
    }

    [Fact]
    public async Task ExecuteAsync_ValidProfile_CompletesExecution()
    {
        var profile = CreateProfile();

        var profiles =
            new FakeProfileRepository(profile);

        var executions =
            new FakeExecutionRepository();

        var runner = new FakeImportRunner();

        var service = new SearchExecutionService(
            profiles,
            executions,
            runner);

        var result = await service.ExecuteAsync(
            CreateRequest(profile));

        Assert.Equal(
            SearchExecutionStatus.Completed,
            result.Status);

        Assert.Equal(profile.Id, result.JobSearchProfileId);

        Assert.Equal("Jooble", result.ProviderName);

        Assert.Equal(10, result.RetrievedCount);
        Assert.Equal(8, result.InsertedCount);
        Assert.Equal(2, result.DuplicateCount);

        Assert.NotNull(result.CompletedAt);
        Assert.Null(result.ErrorMessage);

        Assert.Single(executions.Executions);

        Assert.Equal(2, executions.SaveChangesCount);
    }

    [Fact]
    public async Task ExecuteAsync_UnknownProfile_ThrowsException()
    {
        var executions = new FakeExecutionRepository();

        var service = new SearchExecutionService(
            new FakeProfileRepository(),
            executions,
            new FakeImportRunner());

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.ExecuteAsync(
                new StartSearchRequest(Guid.NewGuid())));

        Assert.Empty(executions.Executions);
    }

    [Fact]
    public async Task ExecuteAsync_DisabledProfile_ThrowsException()
    {
        var profile = CreateProfile();

        profile.Disable();

        var executions = new FakeExecutionRepository();

        var runner = new FakeImportRunner();

        var service = new SearchExecutionService(
            new FakeProfileRepository(profile),
            executions,
            runner);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExecuteAsync(
                CreateRequest(profile)));

        Assert.Empty(executions.Executions);
        Assert.Equal(0, runner.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_PartialImport_SetsPartialStatus()
    {
        var profile = CreateProfile();

        var runner = new FakeImportRunner
        {
            Result = CreateResult(10, 6, 2, 2)
        };

        var service = new SearchExecutionService(
            new FakeProfileRepository(profile),
            new FakeExecutionRepository(),
            runner);

        var result = await service.ExecuteAsync(
            CreateRequest(profile));

        Assert.Equal(
            SearchExecutionStatus.PartiallyCompleted,
            result.Status);

        Assert.Equal(10, result.RetrievedCount);
        Assert.Equal(6, result.InsertedCount);
        Assert.Equal(2, result.DuplicateCount);

        Assert.NotNull(result.ErrorMessage);
        Assert.NotNull(result.CompletedAt);
    }

    [Fact]
    public async Task ExecuteAsync_AllJobsInvalid_SetsFailedStatus()
    {
        var profile = CreateProfile();

        var runner = new FakeImportRunner
        {
            Result = CreateResult(5, 0, 0, 5)
        };

        var service = new SearchExecutionService(
            new FakeProfileRepository(profile),
            new FakeExecutionRepository(),
            runner);

        var result = await service.ExecuteAsync(
            CreateRequest(profile));

        Assert.Equal(
            SearchExecutionStatus.Failed,
            result.Status);

        Assert.Equal(5, result.RetrievedCount);
        Assert.Equal(0, result.InsertedCount);

        Assert.NotNull(result.ErrorMessage);
        Assert.NotNull(result.CompletedAt);
    }

    [Fact]
    public async Task ExecuteAsync_ProviderFailure_PersistsFailedStatus()
    {
        var profile = CreateProfile();

        var executions = new FakeExecutionRepository();

        var runner = new FakeImportRunner
        {
            ExceptionToThrow =
                new InvalidOperationException(
                    "Sensitive provider error.")
        };

        var service = new SearchExecutionService(
            new FakeProfileRepository(profile),
            executions,
            runner);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExecuteAsync(
                CreateRequest(profile)));

        var execution =
            Assert.Single(executions.Executions);

        Assert.Equal(
            SearchExecutionStatus.Failed,
            execution.Status);

        Assert.NotNull(execution.ErrorMessage);

        Assert.DoesNotContain(
            "Sensitive provider error",
            execution.ErrorMessage);

        Assert.NotNull(execution.CompletedAt);

        Assert.Equal(2, executions.SaveChangesCount);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyResults_CompletesExecution()
    {
        var profile = CreateProfile();

        var runner = new FakeImportRunner
        {
            Result = CreateResult(0, 0, 0, 0)
        };

        var service = new SearchExecutionService(
            new FakeProfileRepository(profile),
            new FakeExecutionRepository(),
            runner);

        var result = await service.ExecuteAsync(
            CreateRequest(profile));

        Assert.Equal(
            SearchExecutionStatus.Completed,
            result.Status);

        Assert.Equal(0, result.RetrievedCount);
        Assert.Equal(0, result.InsertedCount);
        Assert.Equal(0, result.DuplicateCount);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsProfileSettings()
    {
        var profile = CreateProfile();

        var runner = new FakeImportRunner();

        var service = new SearchExecutionService(
            new FakeProfileRepository(profile),
            new FakeExecutionRepository(),
            runner);

        await service.ExecuteAsync(
            CreateRequest(profile) with
            {
                MaxResults = 50
            });

        Assert.NotNull(runner.LastRequest);

        Assert.Equal(
            profile.Keywords,
            runner.LastRequest.Keywords);

        Assert.Equal(
            profile.Location,
            runner.LastRequest.Location);

        Assert.Equal(
            profile.IncludeRemote,
            runner.LastRequest.IncludeRemote);

        Assert.Equal(
            50,
            runner.LastRequest.MaxResults);
    }

    [Fact]
    public async Task ExecuteAsync_MissingLocation_UsesHungary()
    {
        var profile = new JobSearchProfile(
            "Developer",
            "C#",
            null);

        var runner = new FakeImportRunner();

        var service = new SearchExecutionService(
            new FakeProfileRepository(profile),
            new FakeExecutionRepository(),
            runner);

        await service.ExecuteAsync(
            CreateRequest(profile));

        Assert.NotNull(runner.LastRequest);

        Assert.Equal(
            "Hungary",
            runner.LastRequest.Location);
    }

    [Fact]
    public async Task ExecuteAsync_ScheduledTrigger_PreservesTrigger()
    {
        var profile = CreateProfile();

        var service = new SearchExecutionService(
            new FakeProfileRepository(profile),
            new FakeExecutionRepository(),
            new FakeImportRunner());

        var result = await service.ExecuteAsync(
            CreateRequest(profile) with
            {
                TriggerType = SearchTriggerType.Scheduled
            });

        Assert.Equal(
            SearchTriggerType.Scheduled,
            result.TriggerType);
    }

    [Fact]
    public async Task ExecuteAsync_CancelledBeforeStart_DoesNotCreateExecution()
    {
        var profile = CreateProfile();

        var executions = new FakeExecutionRepository();

        var service = new SearchExecutionService(
            new FakeProfileRepository(profile),
            executions,
            new FakeImportRunner());

        using var cancellation =
            new CancellationTokenSource();

        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ExecuteAsync(
                CreateRequest(profile),
                cancellation.Token));

        Assert.Empty(executions.Executions);
    }

    [Fact]
    public async Task ExecuteAsync_CancelledDuringImport_PersistsCancelledStatus()
    {
        var profile = CreateProfile();

        var executions = new FakeExecutionRepository();

        using var cancellation =
            new CancellationTokenSource();

        var runner = new FakeImportRunner
        {
            OnImport = cancellation.Cancel
        };

        var service = new SearchExecutionService(
            new FakeProfileRepository(profile),
            executions,
            runner);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ExecuteAsync(
                CreateRequest(profile),
                cancellation.Token));

        var execution =
            Assert.Single(executions.Executions);

        Assert.Equal(
            SearchExecutionStatus.Cancelled,
            execution.Status);

        Assert.NotNull(execution.CompletedAt);

        Assert.Equal(2, executions.SaveChangesCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1001)]
    public async Task ExecuteAsync_InvalidMaxResults_ThrowsException(
        int maxResults)
    {
        var profile = CreateProfile();

        var service = new SearchExecutionService(
            new FakeProfileRepository(profile),
            new FakeExecutionRepository(),
            new FakeImportRunner());

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ExecuteAsync(
                CreateRequest(profile) with
                {
                    MaxResults = maxResults
                }));
    }

    [Fact]
    public async Task ExecuteAsync_ManualProvider_ThrowsException()
    {
        var profile = CreateProfile();

        var service = new SearchExecutionService(
            new FakeProfileRepository(profile),
            new FakeExecutionRepository(),
            new FakeImportRunner());

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ExecuteAsync(
                CreateRequest(profile) with
                {
                    SourceType = JobSourceType.Manual
                }));
    }

    [Fact]
    public async Task GetByIdAsync_ExistingExecution_ReturnsExecution()
    {
        var profile = CreateProfile();

        var service = new SearchExecutionService(
            new FakeProfileRepository(profile),
            new FakeExecutionRepository(),
            new FakeImportRunner());

        var created = await service.ExecuteAsync(
            CreateRequest(profile));

        var result = await service.GetByIdAsync(
            created.Id);

        Assert.NotNull(result);

        Assert.Equal(created.Id, result.Id);
        Assert.Equal(created.Status, result.Status);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownExecution_ReturnsNull()
    {
        var service = new SearchExecutionService(
            new FakeProfileRepository(),
            new FakeExecutionRepository(),
            new FakeImportRunner());

        var result = await service.GetByIdAsync(
            Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetRecentAsync_ReturnsRequestedCount()
    {
        var profile = CreateProfile();

        var service = new SearchExecutionService(
            new FakeProfileRepository(profile),
            new FakeExecutionRepository(),
            new FakeImportRunner());

        for (var i = 0; i < 5; i++)
        {
            await service.ExecuteAsync(
                CreateRequest(profile));
        }

        var result = await service.GetRecentAsync(3);

        Assert.Equal(3, result.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(501)]
    public async Task GetRecentAsync_InvalidCount_ThrowsException(
        int count)
    {
        var service = new SearchExecutionService(
            new FakeProfileRepository(),
            new FakeExecutionRepository(),
            new FakeImportRunner());

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.GetRecentAsync(count));
    }

    private sealed class FakeImportRunner
        : ISearchImportRunner
    {
        public JobImportResult Result { get; set; }
            = CreateResult();

        public Exception? ExceptionToThrow { get; set; }

        public JobProviderSearchRequest? LastRequest
        {
            get;
            private set;
        }

        public int CallCount { get; private set; }

        public Action? OnImport { get; set; }

        public Task<JobImportResult> RunAsync(
            JobSourceType sourceType,
            JobProviderSearchRequest request,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastRequest = request;

            OnImport?.Invoke();

            cancellationToken.ThrowIfCancellationRequested();

            if (ExceptionToThrow is not null)
                throw ExceptionToThrow;

            return Task.FromResult(Result);
        }
    }

    private sealed class FakeProfileRepository
        : IJobSearchProfileRepository
    {
        private readonly List<JobSearchProfile> _profiles;

        public FakeProfileRepository(
            params JobSearchProfile[] profiles)
        {
            _profiles = profiles.ToList();
        }

        public Task<JobSearchProfile?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _profiles.FirstOrDefault(x => x.Id == id));
        }

        public Task<IReadOnlyList<JobSearchProfile>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<
                IReadOnlyList<JobSearchProfile>>(
                _profiles.ToList());
        }

        public Task<IReadOnlyList<JobSearchProfile>> GetEnabledAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<
                IReadOnlyList<JobSearchProfile>>(
                _profiles
                    .Where(x => x.IsEnabled)
                    .ToList());
        }

        public Task AddAsync(
            JobSearchProfile profile,
            CancellationToken cancellationToken = default)
        {
            _profiles.Add(profile);

            return Task.CompletedTask;
        }

        public Task DeleteAsync(
            JobSearchProfile profile,
            CancellationToken cancellationToken = default)
        {
            _profiles.Remove(profile);

            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeExecutionRepository
        : ISearchExecutionRepository
    {
        public List<SearchExecution> Executions { get; }
            = new();

        public int SaveChangesCount { get; private set; }

        public Task<SearchExecution?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Executions.FirstOrDefault(x => x.Id == id));
        }

        public Task<IReadOnlyList<SearchExecution>> GetRecentAsync(
            int count,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<
                IReadOnlyList<SearchExecution>>(
                Executions
                    .OrderByDescending(x => x.StartedAt)
                    .Take(count)
                    .ToList());
        }

        public Task AddAsync(
            SearchExecution execution,
            CancellationToken cancellationToken = default)
        {
            Executions.Add(execution);

            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            SaveChangesCount++;

            return Task.CompletedTask;
        }
    }
}
