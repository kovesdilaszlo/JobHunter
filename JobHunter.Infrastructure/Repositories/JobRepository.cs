
using JobHunter.Application.Interfaces;
using JobHunter.Domain.Entities;
using JobHunter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.Infrastructure.Repositories;

public sealed class JobRepository : IJobRepository
{
    private readonly JobHunterDbContext _context;

    public JobRepository(JobHunterDbContext context)
    {
        _context = context;
    }

    public async Task<JobPosting?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.JobPostings
            .Include(x => x.Sources)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<JobPosting>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.JobPostings
            .Include(x => x.Sources)
            .OrderByDescending(x => x.DiscoveredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        JobPosting job,
        CancellationToken cancellationToken = default)
    {
        await _context.JobPostings.AddAsync(
            job, cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
