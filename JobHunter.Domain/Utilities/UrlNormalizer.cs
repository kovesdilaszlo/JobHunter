
using System.Security.Cryptography;
using System.Text;

namespace JobHunter.Domain.Utilities;

public static class UrlNormalizer
{
    private static readonly HashSet<string> TrackingParameters =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "fbclid",
            "gclid",
            "msclkid"
        };

    public static string Normalize(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp &&
             uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException(
                "A valid HTTP or HTTPS URL is required.",
                nameof(url));
        }

        var builder = new UriBuilder(uri)
        {
            Fragment = string.Empty
        };

        var query = uri.Query.TrimStart('?');

        var parameters = query
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Where(parameter => !IsTrackingParameter(parameter))
            .ToArray();

        builder.Query = string.Join("&", parameters);

        var normalized = builder.Uri;

        var path = normalized.AbsolutePath.TrimEnd('/');

        return normalized.GetLeftPart(UriPartial.Authority)
            + path
            + normalized.Query;
    }

    public static string CreateFingerprint(string url)
    {
        var normalizedUrl = Normalize(url);

        var bytes = Encoding.UTF8.GetBytes(normalizedUrl);

        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash);
    }

    private static bool IsTrackingParameter(string parameter)
    {
        var separatorIndex = parameter.IndexOf('=');

        var rawKey = separatorIndex >= 0
            ? parameter[..separatorIndex]
            : parameter;

        var key = Uri.UnescapeDataString(rawKey);

        return key.StartsWith(
                   "utm_",
                   StringComparison.OrdinalIgnoreCase)
               || TrackingParameters.Contains(key);
    }
}
