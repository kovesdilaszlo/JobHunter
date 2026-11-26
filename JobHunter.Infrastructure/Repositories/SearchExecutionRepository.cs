
using JobHunter.Application.Interfaces;
using JobHunter.Domain.Entities;
using JobHunter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.Infrastructure.Repositories;

public sealed class SearchExecutionRepository
    : ISearchExecutionRepository
{
    private readonly JobHunterDbContext _context;

    public SearchExecutionRepository(
        JobHunterDbContext context)
    {
        _context = context;
    }

    public async Task<SearchExecution?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.SearchExecutions
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<SearchExecution>> GetRecentAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(count));
        }

        return await _context.SearchExecutions
            .AsNoTracking()
            .OrderByDescending(x => x.StartedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        SearchExecution execution,
        CancellationToken cancellationToken = default)
    {
        await _context.SearchExecutions.AddAsync(
            execution, cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
