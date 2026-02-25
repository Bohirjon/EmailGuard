using EmailGuard;

namespace EmailGuard.Tests;

/// <summary>
/// Unit tests for the EmailValidator.Validate() three-step pipeline.
/// Organised by validation step so failures pinpoint the responsible layer.
/// </summary>
public class EmailValidatorTests
{
    // ────────────────────────────────────────────────────────────────────
    // Step 1 – Quick Guard (regex: one @, no whitespace, dot in domain)
    // ────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Validate_NullOrWhitespace_ReturnsInvalidFormat(string? email)
    {
        Assert.Equal(EmailValidationResult.InvalidFormat, EmailValidator.Validate(email));
    }

    [Theory]
    [InlineData("plainaddress")]            // no @ at all
    [InlineData("@missing-local.com")]      // nothing before @
    [InlineData("user@")]                   // nothing after @
    [InlineData("user@domain")]             // no dot in domain
    [InlineData("user @domain.com")]        // space in local part
    [InlineData("user@do main.com")]        // space in domain
    [InlineData("user@ domain.com")]        // space after @
    [InlineData("user@domain .com")]        // space before dot
    public void Validate_ObviousJunk_ReturnsInvalidFormat(string email)
    {
        Assert.Equal(EmailValidationResult.InvalidFormat, EmailValidator.Validate(email));
    }

    [Fact]
    public void Validate_MultipleAtSigns_ReturnsNotValid()
    {
        var result = EmailValidator.Validate("a@b@c.com");
        Assert.NotEqual(EmailValidationResult.Valid, result);
    }

    // ────────────────────────────────────────────────────────────────────
    // Step 2 – RFC 5321/5322 structural validation
    // ────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("user@.example.com")]          // domain starts with dot
    [InlineData("user@example..com")]          // consecutive dots in domain
    [InlineData(".user@example.com")]          // local part starts with dot
    [InlineData("user.@example.com")]          // local part ends with dot
    [InlineData("user..name@example.com")]     // consecutive dots in local part
    public void Validate_RfcViolations_ReturnsNotValid(string email)
    {
        Assert.NotEqual(EmailValidationResult.Valid, EmailValidator.Validate(email));
    }

    [Fact]
    public void Validate_LocalPartExceeds64Chars_ReturnsRfcViolation()
    {
        var longLocal = new string('a', 65);
        var email = $"{longLocal}@example.com";
        Assert.Equal(EmailValidationResult.RfcViolation, EmailValidator.Validate(email));
    }

    [Fact]
    public void Validate_TotalExceeds254Chars_ReturnsRfcViolation()
    {
        var longLocal = new string('a', 64);
        var longDomain = new string('b', 186) + ".com"; // 64 + 1(@) + 190 = 255 > 254
        var email = $"{longLocal}@{longDomain}";
        Assert.Equal(EmailValidationResult.RfcViolation, EmailValidator.Validate(email));
    }

    [Fact]
    public void Validate_DomainLabelExceeds63Chars_ReturnsRfcViolation()
    {
        var longLabel = new string('a', 64);
        var email = $"user@{longLabel}.com";
        Assert.Equal(EmailValidationResult.RfcViolation, EmailValidator.Validate(email));
    }

    [Theory]
    [InlineData("user@-example.com")]          // domain label starts with hyphen
    [InlineData("user@example-.com")]          // domain label ends with hyphen
    public void Validate_DomainHyphenEdges_ReturnsRfcViolation(string email)
    {
        Assert.Equal(EmailValidationResult.RfcViolation, EmailValidator.Validate(email));
    }

    // ────────────────────────────────────────────────────────────────────
    // Step 3 – TLD check (static IANA list)
    // ────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("user@example.invalidtld")]
    [InlineData("user@example.zzzzz")]
    [InlineData("user@example.abcdefg")]
    [InlineData("user@example.notreal")]
    public void Validate_UnknownTld_ReturnsInvalidTld(string email)
    {
        Assert.Equal(EmailValidationResult.InvalidTld, EmailValidator.Validate(email));
    }

    // ────────────────────────────────────────────────────────────────────
    // Happy path – all three steps pass
    // ────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("firstname.lastname@example.org")]
    [InlineData("user+tag@gmail.com")]
    [InlineData("user@sub.domain.co.uk")]
    [InlineData("hello@example.io")]
    [InlineData("contact@company.net")]
    [InlineData("a@b.ai")]
    [InlineData("test@example.dev")]
    public void Validate_ValidEmails_ReturnsValid(string email)
    {
        Assert.Equal(EmailValidationResult.Valid, EmailValidator.Validate(email));
    }

    // ────────────────────────────────────────────────────────────────────
    // IsValid convenience method
    // ────────────────────────────────────────────────────────────────────

    [Fact]
    public void IsValid_ValidEmail_ReturnsTrue()
    {
        Assert.True(EmailValidator.IsValid("user@example.com"));
    }

    [Fact]
    public void IsValid_InvalidEmail_ReturnsFalse()
    {
        Assert.False(EmailValidator.IsValid("not-an-email"));
    }

    [Fact]
    public void IsValid_Null_ReturnsFalse()
    {
        Assert.False(EmailValidator.IsValid(null));
    }

    // ────────────────────────────────────────────────────────────────────
    // Case insensitivity
    // ────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("USER@EXAMPLE.COM")]
    [InlineData("User@Example.Com")]
    [InlineData("user@EXAMPLE.COM")]
    public void Validate_CaseInsensitive_ReturnsValid(string email)
    {
        Assert.Equal(EmailValidationResult.Valid, EmailValidator.Validate(email));
    }

    // ────────────────────────────────────────────────────────────────────
    // Special characters allowed by RFC in local part
    // ────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("user+mailbox@example.com")]
    [InlineData("user.name@example.com")]
    [InlineData("user-name@example.com")]
    [InlineData("user_name@example.com")]
    [InlineData("contact_@live.com")]
    public void Validate_SpecialCharsInLocalPart_ReturnsValid(string email)
    {
        Assert.Equal(EmailValidationResult.Valid, EmailValidator.Validate(email));
    }

    // ────────────────────────────────────────────────────────────────────
    // Numeric and hyphenated domains
    // ────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("user@123.123.com")]
    [InlineData("user@my-domain.com")]
    [InlineData("user@sub.my-domain.org")]
    public void Validate_NumericAndHyphenatedDomains_ReturnsValid(string email)
    {
        Assert.Equal(EmailValidationResult.Valid, EmailValidator.Validate(email));
    }
}

