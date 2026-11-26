
using JobHunter.Domain.Enums;

namespace JobHunter.Application.DTOs;

public sealed record StartSearchRequest(
    Guid SearchProfileId,
    JobSourceType SourceType = JobSourceType.Jooble,
    SearchTriggerType TriggerType = SearchTriggerType.Manual,
    int MaxResults = 100);
