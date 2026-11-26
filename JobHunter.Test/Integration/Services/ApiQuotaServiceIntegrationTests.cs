using JobHunter.Application.DTOs;
using JobHunter.Application.Interfaces;
using JobHunter.Domain.Enums;
using JobHunter.Infrastructure.Persistence;
using JobHunter.Infrastructure.Providers.Jooble;
using JobHunter.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace JobHunter.Test.Integration.Services;

public class ApiQuotaServiceIntegrationTests
{
    private static readonly DateTimeOffset InitialTime =
        new(2026, 11, 26, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetQuotaAsync_NoUsage_ReturnsFullQuota()
    {
        await using var environment =
            new TestEnvironment(100);

        var result =
            await environment.Service.GetQuotaAsync(
                JobSourceType.Jooble);

        Assert.Equal(100, result.DailyLimit);
        Assert.Equal(0, result.RequestCount);
        Assert.Equal(100, result.RemainingCount);

        Assert.False(result.IsExhausted);
    }

    [Fact]
    public async Task GetQuotaAsync_NoUsage_DoesNotCreateRecord()
    {
        await using var environment =
            new TestEnvironment(100);

        await environment.Service.GetQuotaAsync(
            JobSourceType.Jooble);

        Assert.Equal(
            0,
            await environment.GetRecordCountAsync());
    }

    [Fact]
    public async Task TryConsumeAsync_FirstRequest_PersistsUsage()
    {
        await using var environment =
            new TestEnvironment(100);

        var consumed =
            await environment.Service.TryConsumeAsync(
                JobSourceType.Jooble);

        var quota =
            await environment.Service.GetQuotaAsync(
                JobSourceType.Jooble);

        Assert.True(consumed);
        Assert.Equal(1, quota.RequestCount);
        Assert.Equal(99, quota.RemainingCount);

        Assert.Equal(
            1,
            await environment.GetRecordCountAsync());
    }

    [Fact]
    public async Task TryConsumeAsync_MultipleRequests_UpdatesSameRecord()
    {
        await using var environment =
            new TestEnvironment(100);

        for (var i = 0; i < 5; i++)
        {
            Assert.True(
                await environment.Service.TryConsumeAsync(
                    JobSourceType.Jooble));
        }

        var quota =
            await environment.Service.GetQuotaAsync(
                JobSourceType.Jooble);

        Assert.Equal(5, quota.RequestCount);

        Assert.Equal(
            1,
            await environment.GetRecordCountAsync());
    }

    [Fact]
    public async Task TryConsumeAsync_ExhaustedQuota_ReturnsFalse()
    {
        await using var environment =
            new TestEnvironment(2);

        Assert.True(
            await environment.Service.TryConsumeAsync(
                JobSourceType.Jooble));

        Assert.True(
            await environment.Service.TryConsumeAsync(
                JobSourceType.Jooble));

        Assert.False(
            await environment.Service.TryConsumeAsync(
                JobSourceType.Jooble));

        var quota =
            await environment.Service.GetQuotaAsync(
                JobSourceType.Jooble);

        Assert.Equal(2, quota.RequestCount);
        Assert.Equal(0, quota.RemainingCount);
        Assert.True(quota.IsExhausted);
    }

    [Fact]
    public async Task TryConsumeAsync_NewUtcDay_ResetsQuota()
    {
        await using var environment =
            new TestEnvironment(1);

        Assert.True(
            await environment.Service.TryConsumeAsync(
                JobSourceType.Jooble));

        environment.Clock.Advance(
            TimeSpan.FromDays(1));

        var quota =
            await environment.Service.GetQuotaAsync(
                JobSourceType.Jooble);

        Assert.Equal(0, quota.RequestCount);
        Assert.Equal(1, quota.RemainingCount);

        Assert.True(
            await environment.Service.TryConsumeAsync(
                JobSourceType.Jooble));

        Assert.Equal(
            2,
            await environment.GetRecordCountAsync());
    }

    [Fact]
    public async Task TryConsumeAsync_ServiceRecreated_PreservesQuota()
    {
        await using var environment =
            new TestEnvironment(100);

        await environment.Service.TryConsumeAsync(
            JobSourceType.Jooble);

        await environment.Service.TryConsumeAsync(
            JobSourceType.Jooble);

        using var recreatedService =
            environment.CreateAdditionalService();

        var quota =
            await recreatedService.GetQuotaAsync(
                JobSourceType.Jooble);

        Assert.Equal(2, quota.RequestCount);
        Assert.Equal(98, quota.RemainingCount);
    }

    [Fact]
    public async Task TryConsumeAsync_ConcurrentRequests_RespectsLimit()
    {
        await using var environment =
            new TestEnvironment(10);

        var tasks = Enumerable.Range(0, 25)
            .Select(_ => Task.Run(
                () => environment.Service.TryConsumeAsync(
                    JobSourceType.Jooble)))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        Assert.Equal(
            10,
            results.Count(x => x));

        Assert.Equal(
            15,
            results.Count(x => !x));

        var quota =
            await environment.Service.GetQuotaAsync(
                JobSourceType.Jooble);

        Assert.Equal(10, quota.RequestCount);

        Assert.Equal(
            1,
            await environment.GetRecordCountAsync());
    }

    [Fact]
    public async Task TryConsumeAsync_UnsupportedProvider_ThrowsException()
    {
        await using var environment =
            new TestEnvironment(100);

        await Assert.ThrowsAsync<NotSupportedException>(
            () => environment.Service.TryConsumeAsync(
                JobSourceType.LinkedIn));
    }

    [Fact]
    public async Task TryConsumeAsync_CancelledToken_DoesNotConsume()
    {
        await using var environment =
            new TestEnvironment(100);

        using var cancellation =
            new CancellationTokenSource();

        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<
            OperationCanceledException>(
            () => environment.Service.TryConsumeAsync(
                JobSourceType.Jooble,
                cancellation.Token));

        Assert.Equal(
            0,
            await environment.GetRecordCountAsync());
    }

    [Fact]
    public async Task GetQuotaAsync_ReturnsCorrectUsagePercentage()
    {
        await using var environment =
            new TestEnvironment(4);

        await environment.Service.TryConsumeAsync(
            JobSourceType.Jooble);

        var quota =
            await environment.Service.GetQuotaAsync(
                JobSourceType.Jooble);

        Assert.Equal(25.0, quota.UsagePercentage);
    }

    [Fact]
    public async Task GetQuotaAsync_ReturnsNextUtcMidnight()
    {
        await using var environment =
            new TestEnvironment(100);

        var quota =
            await environment.Service.GetQuotaAsync(
                JobSourceType.Jooble);

        Assert.Equal(
            new DateTimeOffset(
                2026, 11, 27, 0, 0, 0,
                TimeSpan.Zero),
            quota.ResetsAt);
    }

    private sealed class MutableTimeProvider
        : TimeProvider
    {
        private DateTimeOffset _currentTime;

        public MutableTimeProvider(
            DateTimeOffset initialTime)
        {
            _currentTime = initialTime;
        }

        public override DateTimeOffset GetUtcNow()
        {
            return _currentTime.ToUniversalTime();
        }

        public void Advance(TimeSpan duration)
        {
            _currentTime =
                _currentTime.Add(duration);
        }
    }

    private sealed class TestEnvironment
        : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private readonly ServiceProvider _provider;

        private readonly int _dailyLimit;

        public MutableTimeProvider Clock { get; }

        public IApiQuotaService Service =>
            _provider.GetRequiredService<
                IApiQuotaService>();

        public TestEnvironment(int dailyLimit)
        {
            _dailyLimit = dailyLimit;

            Clock = new MutableTimeProvider(
                InitialTime);

            _connection = new SqliteConnection(
                "Data Source=:memory:;" +
                "Foreign Keys=True");

            _connection.Open();

            var services = new ServiceCollection();

            services.AddDbContext<JobHunterDbContext>(
                options =>
                    options.UseSqlite(_connection));

            services.AddSingleton<TimeProvider>(
                Clock);

            services.AddSingleton<
                IOptions<JoobleQuotaOptions>>(
                    Options.Create(
                        new JoobleQuotaOptions
                        {
                            DailyLimit = dailyLimit
                        }));

            services.AddSingleton<
                IApiQuotaService,
                ApiQuotaService>();

            _provider = services.BuildServiceProvider();

            using var scope =
                _provider.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<JobHunterDbContext>();

            context.Database.EnsureCreated();
        }

        public ApiQuotaService CreateAdditionalService()
        {
            return new ApiQuotaService(
                _provider.GetRequiredService<
                    IServiceScopeFactory>(),
                Clock,
                Options.Create(
                    new JoobleQuotaOptions
                    {
                        DailyLimit = _dailyLimit
                    }));
        }

        public async Task<int> GetRecordCountAsync()
        {
            await using var scope =
                _provider.CreateAsyncScope();

            var context = scope.ServiceProvider
                .GetRequiredService<JobHunterDbContext>();

            return await context.ApiQuotaUsages
                .CountAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await _provider.DisposeAsync();

            await _connection.DisposeAsync();
        }
    }
}
