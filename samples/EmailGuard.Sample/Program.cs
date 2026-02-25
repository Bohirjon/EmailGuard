using EmailGuard;

Console.WriteLine("╔══════════════════════════════════════╗");
Console.WriteLine("║        EmailGuard — Demo             ║");
Console.WriteLine("╚══════════════════════════════════════╝");
Console.WriteLine();

while (true)
{
    Console.Write("Enter email (or 'q' to quit): ");
    var email = Console.ReadLine();

    if (string.Equals(email, "q", StringComparison.OrdinalIgnoreCase))
        break;

    var result = EmailValidator.Validate(email);

    var message = result switch
    {
        EmailValidationResult.Valid         => "✅ Valid email address.",
        EmailValidationResult.InvalidFormat => "❌ Invalid format (must contain one @, no spaces, and a dot in the domain).",
        EmailValidationResult.RfcViolation  => "❌ RFC 5321/5322 violation (illegal characters, dot positions, or length exceeded).",
        EmailValidationResult.InvalidTld    => "❌ Unrecognized top-level domain.",
        _                                   => "❌ Unknown validation error."
    };

    Console.WriteLine(message);
    Console.WriteLine();
}

