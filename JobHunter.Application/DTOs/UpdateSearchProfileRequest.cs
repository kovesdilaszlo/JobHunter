
namespace JobHunter.Application.DTOs;

public sealed record UpdateSearchProfileRequest(
    string Name,
    string Keywords,
    string? Location,
    bool IncludeRemote);
