
using JobHunter.Domain.Enums;

namespace JobHunter.Domain.Entities;

public sealed class ApiQuotaUsage
{
    private ApiQuotaUsage()
    {
        // Required by EF Core.
    }

    public ApiQuotaUsage(
        JobSourceType sourceType,
        DateOnly usageDate)
    {
        if (!Enum.IsDefined(sourceType) ||
            sourceType == JobSourceType.Manual)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sourceType));
        }

        if (usageDate == default)
        {
            throw new ArgumentException(
                "A valid usage date is required.",
                nameof(usageDate));
        }

        Id = Guid.NewGuid();
        SourceType = sourceType;
        UsageDate = usageDate;
        RequestCount = 0;
    }

    public Guid Id { get; private set; }

    public JobSourceType SourceType { get; private set; }

    public DateOnly UsageDate { get; private set; }

    public int RequestCount { get; private set; }

    public DateTimeOffset? LastRequestAt { get; private set; }

    public int GetRemaining(int dailyLimit)
    {
        ValidateLimit(dailyLimit);

        return Math.Max(
            0,
            dailyLimit - RequestCount);
    }

    public bool TryConsume(
        int dailyLimit,
        DateTimeOffset requestedAt)
    {
        ValidateLimit(dailyLimit);

        if (DateOnly.FromDateTime(
                requestedAt.UtcDateTime) != UsageDate)
        {
            throw new ArgumentException(
                "Request date does not match usage date.",
                nameof(requestedAt));
        }

        if (RequestCount >= dailyLimit)
        {
            return false;
        }

        RequestCount++;

        LastRequestAt = requestedAt.ToUniversalTime();

        return true;
    }

    private static void ValidateLimit(int dailyLimit)
    {
        if (dailyLimit <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dailyLimit),
                "Daily limit must be greater than zero.");
        }
    }
}
