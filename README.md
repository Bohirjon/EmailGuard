# EmailGuard

**Zero-dependency, offline email validation for .NET.**

[![NuGet](https://img.shields.io/nuget/v/EmailGuard.svg)](https://www.nuget.org/packages/EmailGuard)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

EmailGuard validates email addresses through a fast three-step pipeline — no network calls, no external dependencies.

## Features

| Step | What it does | Speed |
|------|-------------|-------|
| **1. Quick Guard** | Compiled regex rejects obvious junk (missing `@`, spaces, no domain dot) | ⚡ ~nanoseconds |
| **2. RFC Validation** | Enforces RFC 5321/5322 rules: atext chars, dot positions, length limits | ⚡ ~microseconds |
| **3. TLD Check** | Verifies the domain's TLD against a static IANA root zone list | ⚡ ~microseconds |

### Highlights

- 🚫 **Zero dependencies** — pure .NET, nothing to install
- 🔌 **Fully offline** — no DNS lookups, no HTTP calls
- 🎯 **Multi-target** — supports `net8.0`, `net9.0`, `net10.0`
- 📋 **Detailed results** — tells you *which step* failed, not just pass/fail
- ⚡ **Fast** — source-generated regex, `Span<T>`-based parsing, `HashSet<T>` TLD lookup

## Installation

```bash
dotnet add package EmailGuard
```

## Quick Start

```csharp
using EmailGuard;

// Simple boolean check
if (EmailValidator.IsValid("user@example.com"))
    Console.WriteLine("Valid!");

// Detailed result
var result = EmailValidator.Validate("user@example.com");
switch (result)
{
    case EmailValidationResult.Valid:
        Console.WriteLine("✅ Valid email address.");
        break;
    case EmailValidationResult.InvalidFormat:
        Console.WriteLine("❌ Bad format.");
        break;
    case EmailValidationResult.RfcViolation:
        Console.WriteLine("❌ RFC 5321/5322 violation.");
        break;
    case EmailValidationResult.InvalidTld:
        Console.WriteLine("❌ Unknown TLD.");
        break;
}
```

## API Reference

### `EmailValidator.Validate(string? email)`

Returns an `EmailValidationResult` enum:

| Value | Meaning |
|-------|---------|
| `Valid` | Passed all three validation steps |
| `InvalidFormat` | Failed the quick regex guard (step 1) |
| `RfcViolation` | Failed RFC 5321/5322 structural checks (step 2) |
| `InvalidTld` | Domain TLD not found in the IANA list (step 3) |

### `EmailValidator.IsValid(string? email)`

Returns `true` if the email passes all validation steps; `false` otherwise.

### `TldValidator.IsValidTld(string domain)`

Returns `true` if the domain's TLD is in the known IANA list.

## RFC Rules Enforced (Step 2)

- Exactly one `@` separating local-part and domain
- Total address length ≤ 254 characters
- Local-part length ≤ 64 characters
- Local-part: only RFC 5322 `atext` characters + dots; no leading, trailing, or consecutive dots
- Domain labels: 1–63 characters each, alphanumeric + hyphens, no leading/trailing hyphens
- At least two domain labels (e.g., `example.com`)

## TLD List (Step 3)

The TLD list is sourced from the [IANA Root Zone Database](https://data.iana.org/TLD/tlds-alpha-by-domain.txt) and is embedded as a static `HashSet<string>`. It includes all country-code, generic, and internationalized (punycode `xn--`) TLDs.

**Last updated:** 2026-02-24

To update, replace the list in `TldValidator.cs` and bump the version.

## Project Structure

```
EmailGuard/
├── src/
│   └── EmailGuard/            # The NuGet package library
├── tests/
│   └── EmailGuard.Tests/      # xUnit unit + smoke tests
├── samples/
│   └── EmailGuard.Sample/     # Console demo app
├── EmailGuard.sln
├── README.md
└── LICENSE
```

## License

[MIT](LICENSE)

