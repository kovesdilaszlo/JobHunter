
using JobHunter.Domain.Entities;
using JobHunter.Domain.Enums;
using Xunit;

namespace JobHunter.Test.Domain.Entities;

public class ApiQuotaUsageTests
{
    private static readonly DateOnly UsageDate =
        new(2026, 11, 26);

    private static readonly DateTimeOffset RequestTime =
        new(2026, 11, 26, 10, 0, 0, TimeSpan.Zero);

    private static ApiQuotaUsage CreateUsage()
    {
        return new ApiQuotaUsage(
            JobSourceType.Jooble,
            UsageDate);
    }

    [Fact]
    public void Constructor_ValidData_CreatesUsage()
    {
        var usage = CreateUsage();

        Assert.NotEqual(Guid.Empty, usage.Id);
        Assert.Equal(JobSourceType.Jooble, usage.SourceType);
        Assert.Equal(UsageDate, usage.UsageDate);
        Assert.Equal(0, usage.RequestCount);
        Assert.Null(usage.LastRequestAt);
    }

    [Fact]
    public void Constructor_InvalidSourceType_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ApiQuotaUsage(
                (JobSourceType)999,
                UsageDate));
    }

    [Fact]
    public void Constructor_ManualSource_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ApiQuotaUsage(
                JobSourceType.Manual,
                UsageDate));
    }

    [Fact]
    public void Constructor_DefaultDate_ThrowsException()
    {
        Assert.Throws<ArgumentException>(
            () => new ApiQuotaUsage(
                JobSourceType.Jooble,
                default));
    }

    [Fact]
    public void TryConsume_AvailableQuota_IncrementsCount()
    {
        var usage = CreateUsage();

        var result = usage.TryConsume(
            100,
            RequestTime);

        Assert.True(result);
        Assert.Equal(1, usage.RequestCount);
        Assert.Equal(RequestTime, usage.LastRequestAt);
    }

    [Fact]
    public void TryConsume_LastAvailableRequest_Succeeds()
    {
        var usage = CreateUsage();

        Assert.True(
            usage.TryConsume(2, RequestTime));

        Assert.True(
            usage.TryConsume(2, RequestTime));

        Assert.Equal(2, usage.RequestCount);
        Assert.Equal(0, usage.GetRemaining(2));
    }

    [Fact]
    public void TryConsume_ExhaustedQuota_ReturnsFalse()
    {
        var usage = CreateUsage();

        usage.TryConsume(1, RequestTime);

        var result = usage.TryConsume(
            1,
            RequestTime.AddMinutes(1));

        Assert.False(result);
        Assert.Equal(1, usage.RequestCount);
        Assert.Equal(RequestTime, usage.LastRequestAt);
    }

    [Fact]
    public void TryConsume_DifferentUtcDate_ThrowsException()
    {
        var usage = CreateUsage();

        Assert.Throws<ArgumentException>(
            () => usage.TryConsume(
                100,
                RequestTime.AddDays(1)));

        Assert.Equal(0, usage.RequestCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void TryConsume_InvalidLimit_ThrowsException(
        int limit)
    {
        var usage = CreateUsage();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => usage.TryConsume(
                limit,
                RequestTime));
    }

    [Fact]
    public void GetRemaining_NewQuota_ReturnsFullLimit()
    {
        var usage = CreateUsage();

        Assert.Equal(100, usage.GetRemaining(100));
    }

    [Fact]
    public void GetRemaining_ConsumedQuota_ReturnsRemaining()
    {
        var usage = CreateUsage();

        usage.TryConsume(100, RequestTime);

        usage.TryConsume(100, RequestTime);

        Assert.Equal(98, usage.GetRemaining(100));
    }

    [Fact]
    public void GetRemaining_ReducedLimit_ReturnsZero()
    {
        var usage = CreateUsage();

        usage.TryConsume(100, RequestTime);

        usage.TryConsume(100, RequestTime);

        Assert.Equal(0, usage.GetRemaining(1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GetRemaining_InvalidLimit_ThrowsException(
        int limit)
    {
        var usage = CreateUsage();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => usage.GetRemaining(limit));
    }

    [Fact]
    public void TryConsume_IncreasedLimit_AllowsMoreRequests()
    {
        var usage = CreateUsage();

        Assert.True(
            usage.TryConsume(1, RequestTime));

        Assert.False(
            usage.TryConsume(1, RequestTime));

        Assert.True(
            usage.TryConsume(2, RequestTime));

        Assert.Equal(2, usage.RequestCount);
    }
}
