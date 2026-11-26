
using JobHunter.Application.Interfaces;

namespace JobHunter.Infrastructure.Concurrency;

public sealed class JobImportLock
    : IJobImportLock, IDisposable
{
    private readonly SemaphoreSlim _semaphore =
        new(1, 1);

    public async Task<IDisposable> AcquireAsync(
        CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);

        return new LockHandle(_semaphore);
    }

    public void Dispose()
    {
        _semaphore.Dispose();
    }

    private sealed class LockHandle : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;

        private int _released;

        public LockHandle(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(
                    ref _released, 1) == 0)
            {
                _semaphore.Release();
            }
        }
    }
}
