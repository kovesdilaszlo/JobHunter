
namespace JobHunter.Infrastructure.Providers.Jooble;

public sealed class JoobleQuotaOptions
{
    public const string SectionName = "Jooble";

    public int DailyLimit { get; set; } = 100;
}
