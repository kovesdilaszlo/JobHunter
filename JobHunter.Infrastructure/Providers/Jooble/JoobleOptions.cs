
namespace JobHunter.Infrastructure.Providers.Jooble;

public sealed class JoobleOptions
{
    public const string SectionName = "Jooble";

    public string ApiKey { get; set; } = string.Empty;

    public int PageSize { get; set; } = 20;

    public int MaxPages { get; set; } = 5;

    public int Radius { get; set; } = 26;

    public int TimeoutSeconds { get; set; } = 30;
}
