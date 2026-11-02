
using JobHunter.Domain.Entities;

namespace JobHunter.Application.Interfaces;

public interface IJobRepository
{
    Task<JobPosting?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<JobPosting>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task AddAsync(
        JobPosting job,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
