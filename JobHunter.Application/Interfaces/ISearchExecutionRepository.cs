
using JobHunter.Domain.Entities;

namespace JobHunter.Application.Interfaces;

public interface ISearchExecutionRepository
{
    Task<SearchExecution?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SearchExecution>> GetRecentAsync(
        int count,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        SearchExecution execution,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
