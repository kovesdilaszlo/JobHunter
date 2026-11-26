
using JobHunter.Application.DTOs;
using JobHunter.Application.Interfaces;
using JobHunter.Domain.Entities;
using JobHunter.Domain.Enums;
using JobHunter.Infrastructure.Persistence;
using JobHunter.Infrastructure.Providers.Jooble;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JobHunter.Infrastructure.Services;

public sealed class ApiQuotaService
    : IApiQuotaService, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly TimeProvider _timeProvider;

    private readonly JoobleQuotaOptions _options;

    private readonly SemaphoreSlim _semaphore =
        new(1, 1);

    public ApiQuotaService(
        IServiceScopeFactory scopeFactory,
        TimeProvider timeProvider,
        IOptions<JoobleQuotaOptions> options)
    {
        _scopeFactory = scopeFactory
            ?? throw new ArgumentNullException(
                nameof(scopeFactory));

        _timeProvider = timeProvider
            ?? throw new ArgumentNullException(
                nameof(timeProvider));

        ArgumentNullException.ThrowIfNull(options);

        _options = options.Value;
    }

    public async Task<bool> TryConsumeAsync(
        JobSourceType sourceType,
        CancellationToken cancellationToken = default)
    {
        var dailyLimit = GetDailyLimit(sourceType);

        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            var now = _timeProvider.GetUtcNow();

            var today = DateOnly.FromDateTime(
                now.UtcDateTime);

            await using var scope =
                _scopeFactory.CreateAsyncScope();

            var context = scope.ServiceProvider
                .GetRequiredService<JobHunterDbContext>();

            var usage = await context.ApiQuotaUsages
                .SingleOrDefaultAsync(
                    x => x.SourceType == sourceType &&
                         x.UsageDate == today,
                    cancellationToken);

            if (usage is null)
            {
                usage = new ApiQuotaUsage(
                    sourceType,
                    today);

                context.ApiQuotaUsages.Add(usage);
            }

            if (!usage.TryConsume(dailyLimit, now))
            {
                return false;
            }

            await context.SaveChangesAsync(
                cancellationToken);

            return true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<ApiQuotaDto> GetQuotaAsync(
        JobSourceType sourceType,
        CancellationToken cancellationToken = default)
    {
        var dailyLimit = GetDailyLimit(sourceType);

        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            var today = DateOnly.FromDateTime(
                _timeProvider.GetUtcNow().UtcDateTime);

            await using var scope =
                _scopeFactory.CreateAsyncScope();

            var context = scope.ServiceProvider
                .GetRequiredService<JobHunterDbContext>();

            var usage = await context.ApiQuotaUsages
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.SourceType == sourceType &&
                         x.UsageDate == today,
                    cancellationToken);

            return new ApiQuotaDto(
                sourceType,
                today,
                dailyLimit,
                usage?.RequestCount ?? 0,
                usage?.LastRequestAt);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private int GetDailyLimit(
        JobSourceType sourceType)
    {
        if (sourceType != JobSourceType.Jooble)
        {
            throw new NotSupportedException(
                $"Quota management is not configured " +
                $"for {sourceType}.");
        }

        if (_options.DailyLimit <= 0)
        {
            throw new InvalidOperationException(
                "Jooble daily limit must be " +
                "greater than zero.");
        }

        return _options.DailyLimit;
    }

    public void Dispose()
    {
        _semaphore.Dispose();
    }
}
