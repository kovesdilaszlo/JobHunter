
namespace JobHunter.Application.DTOs;

public sealed record JobProviderSearchRequest(
    string Keywords,
    string Location,
    bool IncludeRemote,
    int MaxResults = 100);
