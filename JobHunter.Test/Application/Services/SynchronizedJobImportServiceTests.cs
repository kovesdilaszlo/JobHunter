
using JobHunter.Application.DTOs;
using JobHunter.Application.Interfaces;
using JobHunter.Application.Services;
using JobHunter.Domain.Enums;
using JobHunter.Infrastructure.Concurrency;
using Xunit;

namespace JobHunter.Test.Application.Services;

public class SynchronizedJobImportServiceTests
{
    private static readonly TimeSpan Timeout =
        TimeSpan.FromSeconds(5);

    private static JobProviderSearchRequest CreateRequest()
    {
        return new JobProviderSearchRequest(
            "Junior .NET Developer",
            "Budapest",
            true,
            100);
    }

    private static JobImportResult CreateResult()
    {
        var now = DateTimeOffset.UtcNow;

        return new JobImportResult(
            JobSourceType.Jooble,
            1,
            1,
            0,
            0,
            0,
            0,
            now,
            now,
            Array.Empty<JobImportError>());
    }

    [Fact]
    public void Constructor_NullInnerService_ThrowsException()
    {
        using var importLock = new JobImportLock();

        Assert.Throws<ArgumentNullException>(
            () => new SynchronizedJobImportService(
                null!,
                importLock));
    }

    [Fact]
    public void Constructor_NullImportLock_ThrowsException()
    {
        var innerService = new FakeImportService();

        Assert.Throws<ArgumentNullException>(
            () => new SynchronizedJobImportService(
                innerService,
                null!));
    }

    [Fact]
    public async Task ImportAsync_ValidRequest_ReturnsInnerResult()
    {
        using var importLock = new JobImportLock();

        var innerService = new FakeImportService();

        var service = new SynchronizedJobImportService(
            innerService,
            importLock);

        var result = await service.ImportAsync(
            JobSourceType.Jooble,
            CreateRequest());

        Assert.Equal(1, result.InsertedCount);
        Assert.Equal(1, innerService.CallCount);
        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public async Task ImportAsync_NullRequest_ThrowsException()
    {
        using var importLock = new JobImportLock();

        var innerService = new FakeImportService();

        var service = new SynchronizedJobImportService(
            innerService,
            importLock);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.ImportAsync(
                JobSourceType.Jooble,
                null!));

        Assert.Equal(0, innerService.CallCount);
    }

    [Fact]
    public async Task ImportAsync_ForwardsRequestAndToken()
    {
        using var importLock = new JobImportLock();

        var innerService = new FakeImportService();

        var service = new SynchronizedJobImportService(
            innerService,
            importLock);

        var request = CreateRequest();

        using var cancellation =
            new CancellationTokenSource();

        await service.ImportAsync(
            JobSourceType.Jooble,
            request,
            cancellation.Token);

        Assert.Equal(
            JobSourceType.Jooble,
            innerService.LastSourceType);

        Assert.Same(
            request,
            innerService.LastRequest);

        Assert.Equal(
            cancellation.Token,
            innerService.LastCancellationToken);
    }

    [Fact]
    public async Task ImportAsync_CancelledToken_DoesNotCallInnerService()
    {
        using var importLock = new JobImportLock();

        var innerService = new FakeImportService();

        var service = new SynchronizedJobImportService(
            innerService,
            importLock);

        using var cancellation =
            new CancellationTokenSource();

        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<
            OperationCanceledException>(
            () => service.ImportAsync(
                JobSourceType.Jooble,
                CreateRequest(),
                cancellation.Token));

        Assert.Equal(0, innerService.CallCount);
    }

    [Fact]
    public async Task ImportAsync_InnerServiceThrows_ReleasesLock()
    {
        using var importLock = new JobImportLock();

        var innerService = new FakeImportService
        {
            ThrowException = true
        };

        var service = new SynchronizedJobImportService(
            innerService,
            importLock);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ImportAsync(
                JobSourceType.Jooble,
                CreateRequest()));

        innerService.ThrowException = false;

        var result = await service.ImportAsync(
                JobSourceType.Jooble,
                CreateRequest())
            .WaitAsync(Timeout);

        Assert.True(result.IsSuccessful);
        Assert.Equal(2, innerService.CallCount);
    }

    [Fact]
    public async Task ImportAsync_ConcurrentRequests_ExecuteSequentially()
    {
        using var importLock = new JobImportLock();

        var firstStarted =
            new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

        var releaseFirst =
            new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

        var firstInnerService = new FakeImportService
        {
            OnImport = async cancellationToken =>
            {
                firstStarted.TrySetResult(true);

                await releaseFirst.Task.WaitAsync(
                    cancellationToken);
            }
        };

        var secondInnerService =
            new FakeImportService();

        var firstService =
            new SynchronizedJobImportService(
                firstInnerService,
                importLock);

        var secondService =
            new SynchronizedJobImportService(
                secondInnerService,
                importLock);

        var firstTask = firstService.ImportAsync(
            JobSourceType.Jooble,
            CreateRequest());

        try
        {
            await firstStarted.Task.WaitAsync(Timeout);

            var secondTask = secondService.ImportAsync(
                JobSourceType.Jooble,
                CreateRequest());

            Assert.False(secondTask.IsCompleted);
            Assert.Equal(0, secondInnerService.CallCount);

            releaseFirst.TrySetResult(true);

            await Task.WhenAll(
                firstTask,
                secondTask).WaitAsync(Timeout);

            Assert.Equal(1, firstInnerService.CallCount);
            Assert.Equal(1, secondInnerService.CallCount);
        }
        finally
        {
            releaseFirst.TrySetResult(true);

            await firstTask.WaitAsync(Timeout);
        }
    }

    private sealed class FakeImportService
        : IJobImportService
    {
        public int CallCount { get; private set; }

        public bool ThrowException { get; set; }

        public JobSourceType LastSourceType
        {
            get;
            private set;
        }

        public JobProviderSearchRequest? LastRequest
        {
            get;
            private set;
        }

        public CancellationToken LastCancellationToken
        {
            get;
            private set;
        }

        public Func<CancellationToken, Task>? OnImport
        {
            get;
            set;
        }

        public async Task<JobImportResult> ImportAsync(
            JobSourceType sourceType,
            JobProviderSearchRequest request,
            CancellationToken cancellationToken = default)
        {
            CallCount++;

            LastSourceType = sourceType;

            LastRequest = request;

            LastCancellationToken = cancellationToken;

            if (ThrowException)
            {
                throw new InvalidOperationException(
                    "Simulated import failure.");
            }

            if (OnImport is not null)
            {
                await OnImport(cancellationToken);
            }

            return CreateResult();
        }
    }
}
