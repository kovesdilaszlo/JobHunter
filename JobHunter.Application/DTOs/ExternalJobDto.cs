
using JobHunter.Domain.Enums;

namespace JobHunter.Application.DTOs;

public sealed record ExternalJobDto(
    string Title,
    string Company,
    string Location,
    string Description,
    string SourceUrl,
    string? ExternalId = null,
    WorkArrangement WorkArrangement = WorkArrangement.Unknown);
