using ConsoleApp;
using EmailValidator = ConsoleApp.EmailValidator;

while (true)
{
    Console.Write("Enter email: ");
    var email = Console.ReadLine();

    var result = EmailValidator.Validate(email);

    var message = result switch
    {
        EmailValidationResult.Valid        => "✅ Valid email address.",
        EmailValidationResult.InvalidFormat => "❌ Invalid format (must contain one @, no spaces, and a dot in the domain).",
        EmailValidationResult.RfcViolation  => "❌ RFC 5321/5322 violation (illegal characters, dot positions, or length exceeded).",
        EmailValidationResult.InvalidTld    => "❌ Unrecognized top-level domain.",
        _                                   => "❌ Unknown validation error."
    };

    Console.WriteLine(message);
    Console.WriteLine();
}