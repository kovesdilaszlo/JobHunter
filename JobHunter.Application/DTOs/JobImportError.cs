namespace JobHunter.Application.DTOs;

public sealed record JobImportError(
    string? ExternalId,
    string? SourceUrl,
    string Message);
