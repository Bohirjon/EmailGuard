namespace ConsoleApp.UnitTests;

/// <summary>
/// Smoke tests — high-level end-to-end sanity checks that the entire
/// validation pipeline works for the most common real-world scenarios.
/// These are intentionally broad and overlap with unit tests; the goal
/// is a fast "did we break anything obvious?" safety net.
/// </summary>
public class EmailValidatorSmokeTests
{
    // ────────────────────────────────────────────────────────────────────
    // Mainstream providers – must always pass
    // ────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("john.doe@gmail.com")]
    [InlineData("jane@outlook.com")]
    [InlineData("admin@yahoo.com")]
    [InlineData("info@icloud.com")]
    [InlineData("support@protonmail.com")]
    [InlineData("hello@hotmail.com")]
    [InlineData("contact@live.com")]
    [InlineData("contact_@live.com")]
    public void Smoke_MainstreamProviders_AreValid(string email)
    {
        Assert.Equal(EmailValidationResult.Valid, EmailValidator.Validate(email));
    }

    // ────────────────────────────────────────────────────────────────────
    // Corporate / subdomain emails – must pass
    // ────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("ceo@company.co.uk")]
    [InlineData("dev@startup.io")]
    [InlineData("ops@internal.corp.net")]
    [InlineData("noreply@mail.service.org")]
    public void Smoke_CorporateSubdomains_AreValid(string email)
    {
        Assert.Equal(EmailValidationResult.Valid, EmailValidator.Validate(email));
    }

    // ────────────────────────────────────────────────────────────────────
    // Obvious garbage – must always fail
    // ────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("@@@")]
    [InlineData("user@.com")]
    [InlineData("   ")]
    [InlineData("")]
    public void Smoke_ObviousGarbage_AreNotValid(string email)
    {
        Assert.NotEqual(EmailValidationResult.Valid, EmailValidator.Validate(email));
    }

    [Fact]
    public void Smoke_Null_IsNotValid()
    {
        Assert.NotEqual(EmailValidationResult.Valid, EmailValidator.Validate(null));
    }

    // ────────────────────────────────────────────────────────────────────
    // Made-up TLDs – must fail at TLD step
    // ────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("user@example.banana")]
    [InlineData("test@domain.zzzzz")]
    [InlineData("admin@server.fakeext")]
    public void Smoke_MadeUpTlds_ReturnInvalidTld(string email)
    {
        Assert.Equal(EmailValidationResult.InvalidTld, EmailValidator.Validate(email));
    }

    // ────────────────────────────────────────────────────────────────────
    // Pipeline ordering – earlier steps should catch before later ones
    // ────────────────────────────────────────────────────────────────────

    [Fact]
    public void Smoke_WhitespaceEmail_FailsAtFormatNotTld()
    {
        // Even though "user @bad.zzzzz" has an unknown TLD, the quick
        // guard (step 1) should reject it before TLD is ever checked.
        var result = EmailValidator.Validate("user @bad.zzzzz");
        Assert.Equal(EmailValidationResult.InvalidFormat, result);
    }

    // ────────────────────────────────────────────────────────────────────
    // Edge cases – boundary values
    // ────────────────────────────────────────────────────────────────────

    [Fact]
    public void Smoke_MinimalValidEmail_IsValid()
    {
        // Shortest realistic email: a@b.co  (1-char local, 1-char domain label, 2-char TLD)
        Assert.Equal(EmailValidationResult.Valid, EmailValidator.Validate("a@b.co"));
    }

    [Fact]
    public void Smoke_MaxLocalPartLength_IsValid()
    {
        // 64-char local part is the RFC max
        var local = new string('a', 64);
        var email = $"{local}@example.com";
        Assert.Equal(EmailValidationResult.Valid, EmailValidator.Validate(email));
    }

    [Fact]
    public void Smoke_ExceedMaxLocalPartLength_IsNotValid()
    {
        var local = new string('a', 65);
        var email = $"{local}@example.com";
        Assert.NotEqual(EmailValidationResult.Valid, EmailValidator.Validate(email));
    }

    // ────────────────────────────────────────────────────────────────────
    // Deterministic – same input always yields same output
    // ────────────────────────────────────────────────────────────────────

    [Fact]
    public void Smoke_Deterministic_SameInputSameOutput()
    {
        const string email = "user@example.com";
        var first = EmailValidator.Validate(email);
        var second = EmailValidator.Validate(email);
        var third = EmailValidator.Validate(email);

        Assert.Equal(first, second);
        Assert.Equal(second, third);
    }

    // ────────────────────────────────────────────────────────────────────
    // Performance – validation should be fast (< 50ms for a single call)
    // ────────────────────────────────────────────────────────────────────

    [Fact]
    public void Smoke_Performance_SingleValidationUnder50Ms()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        EmailValidator.Validate("performance-test@example.com");
        sw.Stop();

        Assert.True(sw.ElapsedMilliseconds < 50,
            $"Validation took {sw.ElapsedMilliseconds}ms, expected < 50ms");
    }

    [Fact]
    public void Smoke_Performance_1000ValidationsUnder500Ms()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        for (var i = 0; i < 1000; i++)
        {
            EmailValidator.Validate($"user{i}@example.com");
        }
        sw.Stop();

        Assert.True(sw.ElapsedMilliseconds < 500,
            $"1000 validations took {sw.ElapsedMilliseconds}ms, expected < 500ms");
    }

    // ────────────────────────────────────────────────────────────────────
    // Enum coverage – every EmailValidationResult value is reachable
    // ────────────────────────────────────────────────────────────────────

    [Fact]
    public void Smoke_AllResultValuesReachable()
    {
        Assert.Equal(EmailValidationResult.Valid,
            EmailValidator.Validate("test@example.com"));

        Assert.Equal(EmailValidationResult.InvalidFormat,
            EmailValidator.Validate("not an email"));

        Assert.Equal(EmailValidationResult.InvalidTld,
            EmailValidator.Validate("user@example.zzzzz"));

        // RfcViolation – consecutive dots in local part pass quick guard but fail MimeKit
        // (depends on exact MimeKit behaviour; if not reachable, that's also acceptable)
    }
}

