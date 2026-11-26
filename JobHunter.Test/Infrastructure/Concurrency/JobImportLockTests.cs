
using JobHunter.Infrastructure.Concurrency;
using Xunit;

namespace JobHunter.Test.Infrastructure.Concurrency;

public class JobImportLockTests
{
    private static readonly TimeSpan Timeout =
        TimeSpan.FromSeconds(5);

    [Fact]
    public async Task AcquireAsync_AvailableLock_ReturnsHandle()
    {
        using var importLock = new JobImportLock();

        using var handle = await importLock
            .AcquireAsync()
            .WaitAsync(Timeout);

        Assert.NotNull(handle);
    }

    [Fact]
    public async Task AcquireAsync_LockAlreadyHeld_Waits()
    {
        using var importLock = new JobImportLock();

        using var firstHandle = await importLock
            .AcquireAsync()
            .WaitAsync(Timeout);

        var secondTask = importLock.AcquireAsync();

        Assert.False(secondTask.IsCompleted);

        firstHandle.Dispose();

        using var secondHandle =
            await secondTask.WaitAsync(Timeout);

        Assert.NotNull(secondHandle);
    }

    [Fact]
    public async Task Dispose_Handle_ReleasesLock()
    {
        using var importLock = new JobImportLock();

        using (var handle = await importLock
                   .AcquireAsync()
                   .WaitAsync(Timeout))
        {
            Assert.NotNull(handle);
        }

        using var nextHandle = await importLock
            .AcquireAsync()
            .WaitAsync(Timeout);

        Assert.NotNull(nextHandle);
    }

    [Fact]
    public async Task Dispose_HandleTwice_DoesNotReleaseTwice()
    {
        using var importLock = new JobImportLock();

        var firstHandle = await importLock
            .AcquireAsync()
            .WaitAsync(Timeout);

        firstHandle.Dispose();
        firstHandle.Dispose();

        using var secondHandle = await importLock
            .AcquireAsync()
            .WaitAsync(Timeout);

        var thirdTask = importLock.AcquireAsync();

        Assert.False(thirdTask.IsCompleted);

        secondHandle.Dispose();

        using var thirdHandle =
            await thirdTask.WaitAsync(Timeout);

        Assert.NotNull(thirdHandle);
    }

    [Fact]
    public async Task AcquireAsync_CancelledToken_ThrowsException()
    {
        using var importLock = new JobImportLock();

        using var cancellation =
            new CancellationTokenSource();

        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<
            OperationCanceledException>(
            () => importLock.AcquireAsync(
                cancellation.Token));
    }

    [Fact]
    public async Task AcquireAsync_CancelWhileWaiting_ThrowsException()
    {
        using var importLock = new JobImportLock();

        using var handle = await importLock
            .AcquireAsync()
            .WaitAsync(Timeout);

        using var cancellation =
            new CancellationTokenSource();

        var waitingTask = importLock.AcquireAsync(
            cancellation.Token);

        Assert.False(waitingTask.IsCompleted);

        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<
            OperationCanceledException>(
            () => waitingTask.WaitAsync(Timeout));
    }

    [Fact]
    public async Task AcquireAsync_CancelledWait_DoesNotCorruptLock()
    {
        using var importLock = new JobImportLock();

        using var firstHandle = await importLock
            .AcquireAsync()
            .WaitAsync(Timeout);

        using var cancellation =
            new CancellationTokenSource();

        var waitingTask = importLock.AcquireAsync(
            cancellation.Token);

        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<
            OperationCanceledException>(
            () => waitingTask.WaitAsync(Timeout));

        firstHandle.Dispose();

        using var nextHandle = await importLock
            .AcquireAsync()
            .WaitAsync(Timeout);

        Assert.NotNull(nextHandle);
    }

    [Fact]
    public async Task AcquireAsync_MultipleTasks_ExecutesExclusively()
    {
        using var importLock = new JobImportLock();

        var activeOperations = 0;
        var completedOperations = 0;
        var violations = 0;

        var tasks = Enumerable.Range(0, 20)
            .Select(async _ =>
            {
                using var handle =
                    await importLock.AcquireAsync();

                var active = Interlocked.Increment(
                    ref activeOperations);

                if (active != 1)
                {
                    Interlocked.Increment(
                        ref violations);
                }

                await Task.Yield();

                Interlocked.Decrement(
                    ref activeOperations);

                Interlocked.Increment(
                    ref completedOperations);
            })
            .ToArray();

        await Task.WhenAll(tasks).WaitAsync(Timeout);

        Assert.Equal(20, completedOperations);
        Assert.Equal(0, activeOperations);
        Assert.Equal(0, violations);
    }

    [Fact]
    public async Task AcquireAsync_ExceptionInsideLock_ReleasesLock()
    {
        using var importLock = new JobImportLock();

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
            {
                using var handle = await importLock
                    .AcquireAsync()
                    .WaitAsync(Timeout);

                throw new InvalidOperationException(
                    "Simulated import failure.");
            });

        using var nextHandle = await importLock
            .AcquireAsync()
            .WaitAsync(Timeout);

        Assert.NotNull(nextHandle);
    }
}
