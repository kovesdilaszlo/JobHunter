
namespace JobHunter.Application.DTOs;

public sealed record CreateSearchProfileRequest(
    string Name,
    string Keywords,
    string? Location,
    bool IncludeRemote = true);
