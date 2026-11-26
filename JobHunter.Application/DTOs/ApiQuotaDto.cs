
using JobHunter.Domain.Enums;

namespace JobHunter.Application.DTOs;

public sealed record ApiQuotaDto(
    JobSourceType SourceType,
    DateOnly UsageDate,
    int DailyLimit,
    int RequestCount,
    DateTimeOffset? LastRequestAt)
{
    public int RemainingCount =>
        Math.Max(0, DailyLimit - RequestCount);

    public bool IsExhausted =>
        RequestCount >= DailyLimit;

    public double UsagePercentage =>
        DailyLimit > 0
            ? RequestCount * 100.0 / DailyLimit
            : 0;

    public DateTimeOffset ResetsAt =>
        new DateTimeOffset(
            UsageDate.AddDays(1)
                .ToDateTime(TimeOnly.MinValue),
            TimeSpan.Zero);
}
