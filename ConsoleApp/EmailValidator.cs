#nullable enable

using System.Text.RegularExpressions;
using MimeKit;

namespace ConsoleApp;

/// <summary>
/// Three-step offline email validator:
///   Step 1 – Quick compiled regex guard (rejects obvious junk)
///   Step 2 – RFC 5321/5322 validation via MimeKit
///   Step 3 – TLD check against a static IANA list
/// </summary>
public static partial class EmailValidator
{
    // Step 1: Source-generated compiled regex — one @, no whitespace, at least one dot in domain.
    [GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.IgnoreCase)]
    private static partial Regex QuickGuardRegex();

    /// <summary>
    /// Validates an email address through all three steps and returns a detailed result.
    /// </summary>
    public static EmailValidationResult Validate(string? email)
    {
        // ── Step 1: Quick guard ──────────────────────────────────────────
        if (string.IsNullOrWhiteSpace(email) || !QuickGuardRegex().IsMatch(email))
            return EmailValidationResult.InvalidFormat;

        // ── Step 2: RFC 5321/5322 validation via MimeKit ─────────────────
        if (!MailboxAddress.TryParse(email, out _))
            return EmailValidationResult.RfcViolation;

        // RFC 5321 length constraints (MimeKit doesn't enforce these)
        var atIndex = email.LastIndexOf('@');
        var localPart = email[..atIndex];
        var domain = email[(atIndex + 1)..];

        if (localPart.Length > 64)
            return EmailValidationResult.RfcViolation;

        if (email.Length > 254)
            return EmailValidationResult.RfcViolation;

        if (!TldValidator.IsValidTld(domain))
            return EmailValidationResult.InvalidTld;

        return EmailValidationResult.Valid;
    }
}


