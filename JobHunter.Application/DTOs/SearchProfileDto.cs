
namespace JobHunter.Application.DTOs;

public sealed record SearchProfileDto(
    Guid Id,
    string Name,
    string Keywords,
    string? Location,
    bool IncludeRemote,
    bool IsEnabled,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
