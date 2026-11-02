
using JobHunter.Domain.Entities;

namespace JobHunter.Application.Interfaces;

public interface IJobSearchProfileRepository
{
    Task<JobSearchProfile?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<JobSearchProfile>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<JobSearchProfile>> GetEnabledAsync(
        CancellationToken cancellationToken = default);

    Task AddAsync(
        JobSearchProfile profile,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        JobSearchProfile profile,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
