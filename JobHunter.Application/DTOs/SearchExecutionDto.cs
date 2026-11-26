
using JobHunter.Domain.Enums;

namespace JobHunter.Application.DTOs;

public sealed record SearchExecutionDto(
    Guid Id,
    Guid JobSearchProfileId,
    string ProviderName,
    SearchTriggerType TriggerType,
    SearchExecutionStatus Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    int RetrievedCount,
    int InsertedCount,
    int DuplicateCount,
    string? ErrorMessage)
{
    public TimeSpan? Duration =>
        CompletedAt.HasValue
            ? CompletedAt.Value - StartedAt
            : null;
}
