#nullable enable

using System.Text.RegularExpressions;

namespace EmailGuard;

/// <summary>
/// Three-step offline email validator (zero external dependencies):
/// <list type="number">
///   <item><description>Quick compiled regex guard — rejects obvious junk</description></item>
///   <item><description>RFC 5321/5322 structural validation — enforces standard rules</description></item>
///   <item><description>TLD check against a static IANA list — rejects made-up domains</description></item>
/// </list>
/// </summary>
/// <example>
/// <code>
/// var result = EmailValidator.Validate("user@example.com");
/// if (result == EmailValidationResult.Valid)
///     Console.WriteLine("Email is valid!");
/// </code>
/// </example>
public static partial class EmailValidator
{
    // Step 1: Source-generated compiled regex — one @, no whitespace, at least one dot in domain.
    [GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.IgnoreCase)]
    private static partial Regex QuickGuardRegex();

    // RFC 5322 atext characters allowed in the local part (unquoted).
    // Letters, digits, and: ! # $ % & ' * + - / = ? ^ _ ` { | } ~
    private static bool IsAtext(char c) =>
        c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or (>= '0' and <= '9')
            or '!' or '#' or '$' or '%' or '&' or '\'' or '*' or '+' or '-'
            or '/' or '=' or '?' or '^' or '_' or '`' or '{' or '|' or '}' or '~';

    /// <summary>
    /// Validates an email address through all three steps and returns a detailed result.
    /// </summary>
    /// <param name="email">The email address to validate. May be <see langword="null"/>.</param>
    /// <returns>
    /// An <see cref="EmailValidationResult"/> indicating the outcome.
    /// <see cref="EmailValidationResult.Valid"/> means the address passed every check.
    /// </returns>
    public static EmailValidationResult Validate(string? email)
    {
        // ── Step 1: Quick guard ──────────────────────────────────────────
        if (string.IsNullOrWhiteSpace(email) || !QuickGuardRegex().IsMatch(email))
            return EmailValidationResult.InvalidFormat;

        // ── Step 2: RFC 5321/5322 structural validation ──────────────────
        if (!PassesRfcValidation(email))
            return EmailValidationResult.RfcViolation;

        // ── Step 3: TLD check against the static IANA list ───────────────
        var atIndex = email.LastIndexOf('@');
        var domain = email[(atIndex + 1)..];

        if (!TldValidator.IsValidTld(domain))
            return EmailValidationResult.InvalidTld;

        return EmailValidationResult.Valid;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the email address is valid; otherwise <see langword="false"/>.
    /// This is a convenience wrapper around <see cref="Validate"/>.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns><see langword="true"/> when the address passes all validation steps.</returns>
    public static bool IsValid(string? email) =>
        Validate(email) == EmailValidationResult.Valid;

    /// <summary>
    /// RFC 5321/5322 structural validation — enforces:
    ///   • Exactly one unquoted @ separating local-part and domain
    ///   • Total length ≤ 254, local-part ≤ 64
    ///   • Local-part: only atext + dots; no leading/trailing/consecutive dots
    ///   • Domain: labels separated by dots; each 1-63 chars; alphanumeric + hyphens;
    ///     no leading/trailing hyphens; at least two labels
    /// </summary>
    private static bool PassesRfcValidation(string email)
    {
        // Total length per RFC 5321 (path limit 256 minus < > = 254)
        if (email.Length > 254)
            return false;

        var atIndex = email.LastIndexOf('@');
        if (atIndex <= 0 || atIndex == email.Length - 1)
            return false;

        // Reject multiple @ signs (LastIndexOf vs IndexOf)
        if (email.IndexOf('@') != atIndex)
            return false;

        var localPart = email.AsSpan(0, atIndex);
        var domain = email.AsSpan(atIndex + 1);

        return IsValidLocalPart(localPart) && IsValidDomain(domain);
    }

    /// <summary>
    /// Validates the local-part (before @) per RFC 5321/5322:
    ///   • Length 1–64
    ///   • Only atext characters and dots
    ///   • No leading, trailing, or consecutive dots
    /// </summary>
    private static bool IsValidLocalPart(ReadOnlySpan<char> local)
    {
        if (local.Length == 0 || local.Length > 64)
            return false;

        // No leading or trailing dot
        if (local[0] == '.' || local[^1] == '.')
            return false;

        var prevWasDot = false;
        foreach (var c in local)
        {
            if (c == '.')
            {
                if (prevWasDot) return false; // consecutive dots
                prevWasDot = true;
            }
            else
            {
                if (!IsAtext(c)) return false;
                prevWasDot = false;
            }
        }

        return true;
    }

    /// <summary>
    /// Validates the domain (after @) per RFC 5321:
    ///   • Split by dots into labels
    ///   • Each label: 1–63 characters, alphanumeric + hyphens only
    ///   • No label starts or ends with a hyphen
    ///   • At least two labels (e.g. "example.com", not just "localhost")
    ///   • No empty labels (no leading/trailing/consecutive dots)
    /// </summary>
    private static bool IsValidDomain(ReadOnlySpan<char> domain)
    {
        if (domain.Length == 0 || domain.Length > 253)
            return false;

        // No leading or trailing dot
        if (domain[0] == '.' || domain[^1] == '.')
            return false;

        var labelCount = 0;
        var labelStart = 0;

        for (var i = 0; i <= domain.Length; i++)
        {
            if (i == domain.Length || domain[i] == '.')
            {
                var label = domain[labelStart..i];

                if (label.Length == 0 || label.Length > 63)
                    return false;

                // No leading/trailing hyphen
                if (label[0] == '-' || label[^1] == '-')
                    return false;

                // Only alphanumeric + hyphens
                foreach (var c in label)
                {
                    if (c is not ((>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or (>= '0' and <= '9') or '-'))
                        return false;
                }

                labelCount++;
                labelStart = i + 1;
            }
        }

        // Must have at least two labels (e.g. "example" + "com")
        return labelCount >= 2;
    }
}

