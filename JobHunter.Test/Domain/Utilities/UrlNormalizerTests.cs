
using JobHunter.Domain.Utilities;

namespace JobHunter.Test.Domain.Utilities;

public class UrlNormalizerTests
{
    [Theory]
    [InlineData(
        "https://example.com/jobs/123",
        "https://example.com/jobs/123")]
    [InlineData(
        "https://example.com/jobs/123/",
        "https://example.com/jobs/123")]
    [InlineData(
        "https://example.com/jobs/123#details",
        "https://example.com/jobs/123")]
    [InlineData(
        "https://example.com/jobs/123?utm_source=google",
        "https://example.com/jobs/123")]
    [InlineData(
        "https://example.com/jobs/123?fbclid=abc",
        "https://example.com/jobs/123")]
    [InlineData(
        "https://example.com/jobs/123?gclid=abc",
        "https://example.com/jobs/123")]
    [InlineData(
        "https://example.com/jobs/123?msclkid=abc",
        "https://example.com/jobs/123")]
    [InlineData(
        "https://example.com/jobs/123?id=123&utm_source=google",
        "https://example.com/jobs/123?id=123")]
    [InlineData(
        "https://EXAMPLE.COM/jobs/123",
        "https://example.com/jobs/123")]
    [InlineData(
        "https://example.com/jobs/123/?id=123",
        "https://example.com/jobs/123?id=123")]
    [InlineData(
        "https://example.com/jobs?source=123",
        "https://example.com/jobs?source=123")]
    public void Normalize_ValidUrl_ReturnsExpected(
        string input,
        string expected)
    {
        var result = UrlNormalizer.Normalize(input);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-url")]
    [InlineData("ftp://example.com/job")]
    [InlineData("javascript:alert(1)")]
    public void Normalize_InvalidUrl_ThrowsException(
        string url)
    {
        Assert.Throws<ArgumentException>(() =>
            UrlNormalizer.Normalize(url));
    }

    [Fact]
    public void CreateFingerprint_ValidUrl_Returns64Characters()
    {
        var fingerprint = UrlNormalizer.CreateFingerprint(
            "https://example.com/jobs/123");

        Assert.Equal(64, fingerprint.Length);

        Assert.Matches(
            "^[A-F0-9]{64}$",
            fingerprint);
    }

    [Fact]
    public void CreateFingerprint_SameUrl_ReturnsSameHash()
    {
        var first = UrlNormalizer.CreateFingerprint(
            "https://example.com/jobs/123");

        var second = UrlNormalizer.CreateFingerprint(
            "https://example.com/jobs/123");

        Assert.Equal(first, second);
    }

    [Fact]
    public void CreateFingerprint_TrackingUrl_ReturnsSameHash()
    {
        var first = UrlNormalizer.CreateFingerprint(
            "https://example.com/jobs/123");

        var second = UrlNormalizer.CreateFingerprint(
            "https://example.com/jobs/123?utm_source=google");

        Assert.Equal(first, second);
    }

    [Fact]
    public void CreateFingerprint_DifferentUrls_ReturnDifferentHashes()
    {
        var first = UrlNormalizer.CreateFingerprint(
            "https://example.com/jobs/123");

        var second = UrlNormalizer.CreateFingerprint(
            "https://example.com/jobs/456");

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void CreateFingerprint_DifferentJobIds_ReturnDifferentHashes()
    {
        var first = UrlNormalizer.CreateFingerprint(
            "https://example.com/jobs?id=123");

        var second = UrlNormalizer.CreateFingerprint(
            "https://example.com/jobs?id=456");

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void CreateFingerprint_DifferentFragments_ReturnSameHash()
    {
        var first = UrlNormalizer.CreateFingerprint(
            "https://example.com/jobs/123#description");

        var second = UrlNormalizer.CreateFingerprint(
            "https://example.com/jobs/123#details");

        Assert.Equal(first, second);
    }
}
