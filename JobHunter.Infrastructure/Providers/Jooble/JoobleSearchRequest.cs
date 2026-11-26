
using System.Text.Json.Serialization;

namespace JobHunter.Infrastructure.Providers.Jooble;

public sealed class JoobleSearchRequest
{
    [JsonPropertyName("keywords")]
    public string Keywords { get; init; } = string.Empty;

    [JsonPropertyName("location")]
    public string Location { get; init; } = string.Empty;

    [JsonPropertyName("radius")]
    public string Radius { get; init; } = "26";

    [JsonPropertyName("page")]
    public int Page { get; init; } = 1;

    [JsonPropertyName("ResultOnPage")]
    public int ResultOnPage { get; init; } = 20;

    [JsonPropertyName("companysearch")]
    public bool CompanySearch { get; init; }
}
