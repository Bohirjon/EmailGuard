namespace ConsoleApp.UnitTests;

/// <summary>
/// Unit tests for the TldValidator static helper.
/// </summary>
public class TldValidatorTests
{
    // ────────────────────────────────────────────────────────────────────
    // Null / empty / whitespace guard
    // ────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValidTld_NullOrWhiteSpace_ReturnsFalse(string? domain)
    {
        Assert.False(TldValidator.IsValidTld(domain!));
    }

    // ────────────────────────────────────────────────────────────────────
    // No dot in domain
    // ────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("localhost")]
    [InlineData("com")]
    [InlineData("example")]
    public void IsValidTld_NoDotInDomain_ReturnsFalse(string domain)
    {
        Assert.False(TldValidator.IsValidTld(domain));
    }

    // ────────────────────────────────────────────────────────────────────
    // Trailing dot (no TLD after the dot)
    // ────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("example.")]
    [InlineData("domain.com.")]
    public void IsValidTld_TrailingDot_ReturnsFalse(string domain)
    {
        Assert.False(TldValidator.IsValidTld(domain));
    }

    // ────────────────────────────────────────────────────────────────────
    // Well-known valid TLDs (country-code & generic)
    // ────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("example.com")]
    [InlineData("example.org")]
    [InlineData("example.net")]
    [InlineData("example.io")]
    [InlineData("example.ai")]
    [InlineData("example.dev")]
    [InlineData("example.app")]
    [InlineData("example.edu")]
    [InlineData("example.gov")]
    [InlineData("example.co")]
    [InlineData("example.uk")]
    [InlineData("example.de")]
    [InlineData("example.fr")]
    [InlineData("example.jp")]
    [InlineData("example.ru")]
    [InlineData("example.us")]
    [InlineData("example.info")]
    [InlineData("example.biz")]
    public void IsValidTld_KnownTlds_ReturnsTrue(string domain)
    {
        Assert.True(TldValidator.IsValidTld(domain));
    }

    // ────────────────────────────────────────────────────────────────────
    // Case insensitivity
    // ────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("example.COM")]
    [InlineData("example.Com")]
    [InlineData("example.cOm")]
    public void IsValidTld_CaseInsensitive_ReturnsTrue(string domain)
    {
        Assert.True(TldValidator.IsValidTld(domain));
    }

    // ────────────────────────────────────────────────────────────────────
    // Made-up TLDs
    // ────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("example.invalidtld")]
    [InlineData("example.zzzzz")]
    [InlineData("example.fakeext")]
    [InlineData("example.notreal")]
    [InlineData("example.abcdefg")]
    public void IsValidTld_UnknownTlds_ReturnsFalse(string domain)
    {
        Assert.False(TldValidator.IsValidTld(domain));
    }

    // ────────────────────────────────────────────────────────────────────
    // Subdomains – TLD extraction uses the last dot
    // ────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("sub.domain.com")]
    [InlineData("a.b.c.org")]
    [InlineData("deep.nested.example.net")]
    public void IsValidTld_SubdomainWithValidTld_ReturnsTrue(string domain)
    {
        Assert.True(TldValidator.IsValidTld(domain));
    }

    [Theory]
    [InlineData("sub.domain.invalidtld")]
    [InlineData("a.b.c.zzzzz")]
    public void IsValidTld_SubdomainWithInvalidTld_ReturnsFalse(string domain)
    {
        Assert.False(TldValidator.IsValidTld(domain));
    }

    // ────────────────────────────────────────────────────────────────────
    // Brand / new gTLDs (verify they're in the list)
    // ────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("test.google")]
    [InlineData("test.amazon")]
    [InlineData("test.apple")]
    [InlineData("test.microsoft")]
    [InlineData("test.cloud")]
    [InlineData("test.shop")]
    [InlineData("test.online")]
    [InlineData("test.xyz")]
    [InlineData("test.tech")]
    public void IsValidTld_BrandAndNewGtlds_ReturnsTrue(string domain)
    {
        Assert.True(TldValidator.IsValidTld(domain));
    }

    // ────────────────────────────────────────────────────────────────────
    // Internationalised TLDs (xn-- punycode)
    // ────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("example.xn--fiqs8s")]       // .中国
    [InlineData("example.xn--p1ai")]         // .рф
    [InlineData("example.xn--j6w193g")]      // .香港
    public void IsValidTld_InternationalisedPunycode_ReturnsTrue(string domain)
    {
        Assert.True(TldValidator.IsValidTld(domain));
    }
}

