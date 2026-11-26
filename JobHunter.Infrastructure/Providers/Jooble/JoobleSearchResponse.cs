
using System.Text.Json.Serialization;

namespace JobHunter.Infrastructure.Providers.Jooble;

public sealed class JoobleSearchResponse
{
    [JsonPropertyName("totalCount")]
    public int TotalCount { get; init; }

    [JsonPropertyName("jobs")]
    public List<JoobleJobResponse>? Jobs { get; init; }
}
