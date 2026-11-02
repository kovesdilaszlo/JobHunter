
using JobHunter.Domain.Enums;

namespace JobHunter.Application.DTOs;

public sealed record JobDto(
    Guid Id,
    string Title,
    string Company,
    string Location,
    string Description,
    WorkArrangement WorkArrangement,
    ApplicationStatus Status,
    DateTimeOffset DiscoveredAt,
    bool IsArchived,
    IReadOnlyList<JobSourceDto> Sources);
