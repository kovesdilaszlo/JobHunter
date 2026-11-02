
using JobHunter.Domain.Enums;

namespace JobHunter.Application.DTOs;

public sealed record JobSourceDto(
    Guid Id,
    JobSourceType SourceType,
    string OriginalUrl,
    string? ExternalId);
