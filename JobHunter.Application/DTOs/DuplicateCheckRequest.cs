
using JobHunter.Domain.Enums;

namespace JobHunter.Application.DTOs;

public sealed record DuplicateCheckRequest(
    JobSourceType SourceType,
    string? ExternalId,
    string SourceUrl,
    string Title,
    string Company,
    string Location);
