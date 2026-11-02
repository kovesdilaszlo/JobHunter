
using JobHunter.Domain.Enums;

namespace JobHunter.Application.DTOs;

public sealed record CreateJobRequest(
    string Title,
    string Company,
    string Location,
    string Description,
    WorkArrangement WorkArrangement,
    JobSourceType SourceType,
    string SourceUrl,
    string? ExternalId = null);
