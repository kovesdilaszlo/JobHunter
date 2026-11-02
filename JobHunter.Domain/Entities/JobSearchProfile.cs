
namespace JobHunter.Domain.Entities;

public class JobSearchProfile
{
    private JobSearchProfile()
    {
        // Required by EF Core.
    }

    public JobSearchProfile(
        string name,
        string keywords,
        string? location,
        bool includeRemote = true)
    {
        Id = Guid.NewGuid();

        Name = RequireText(name, nameof(name));
        Keywords = RequireText(keywords, nameof(keywords));
        Location = NormalizeOptionalText(location);

        IncludeRemote = includeRemote;
        IsEnabled = true;

        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Keywords { get; private set; } = string.Empty;

    public string? Location { get; private set; }

    public bool IncludeRemote { get; private set; }

    public bool IsEnabled { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(
        string name,
        string keywords,
        string? location,
        bool includeRemote)
    {
        var normalizedName = RequireText(name, nameof(name));
        var normalizedKeywords = RequireText(
            keywords, nameof(keywords));

        Name = normalizedName;
        Keywords = normalizedKeywords;
        Location = NormalizeOptionalText(location);
        IncludeRemote = includeRemote;

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Enable()
    {
        if (IsEnabled)
            return;

        IsEnabled = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Disable()
    {
        if (!IsEnabled)
            return;

        IsEnabled = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string RequireText(
        string? value,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Value cannot be empty.",
                parameterName);
        }

        return value.Trim();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
