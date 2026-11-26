
namespace JobHunter.Application.Interfaces;

public interface IJobImportLock
{
    Task<IDisposable> AcquireAsync(
        CancellationToken cancellationToken = default);
}
