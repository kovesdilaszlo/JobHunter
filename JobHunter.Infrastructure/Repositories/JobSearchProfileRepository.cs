
using JobHunter.Application.Interfaces;
using JobHunter.Domain.Entities;
using JobHunter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.Infrastructure.Repositories;

public sealed class JobSearchProfileRepository
    : IJobSearchProfileRepository
{
    private readonly JobHunterDbContext _context;

    public JobSearchProfileRepository(
        JobHunterDbContext context)
    {
        _context = context;
    }

    public async Task<JobSearchProfile?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.JobSearchProfiles
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<JobSearchProfile>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.JobSearchProfiles
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<JobSearchProfile>> GetEnabledAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.JobSearchProfiles
            .Where(x => x.IsEnabled)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        JobSearchProfile profile,
        CancellationToken cancellationToken = default)
    {
        await _context.JobSearchProfiles.AddAsync(
            profile, cancellationToken);
    }

    public Task DeleteAsync(
        JobSearchProfile profile,
        CancellationToken cancellationToken = default)
    {
        _context.JobSearchProfiles.Remove(profile);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
