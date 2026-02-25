namespace EmailGuard;

/// <summary>
/// The result of an email validation, indicating which step (if any) failed.
/// </summary>
public enum EmailValidationResult
{
    /// <summary>The email address is valid.</summary>
    Valid,

    /// <summary>The email failed the quick-format check (missing @, whitespace, no domain dot, etc.).</summary>
    InvalidFormat,

    /// <summary>The email violates RFC 5321/5322 structural rules (illegal characters, dot positions, length limits).</summary>
    RfcViolation,

    /// <summary>The domain's top-level domain is not in the known IANA TLD list.</summary>
    InvalidTld
}
