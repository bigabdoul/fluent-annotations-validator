# FluentAnnotationsValidator

A fluent, type-safe validation engine for .NET that transforms `[ValidationAttribute]` annotations into runtime validation logic. Supports conditional rules, culture-scoped messages, and discoverable configuration via DSL.

## Features

- Automatic validation from `[Required]`, `[Range]`, `[StringLength]`, etc.
- Runtime error message resolution with localization fallback
- `.resx`-based resource binding with auto-assigned `CultureInfo`
- DSL configuration: target types, cultures, resource mapping, conventions

## Quick Setup
Certainly — here’s a refined version that’s clearer, more discoverable, and contributor-friendly. It emphasizes the fluent entry point, scoped configuration, and conditional override with dynamic localization fallback:

---

### Fluent Validation Configuration

Start with automatic rule generation via `[ValidationAttribute]`, then layer culture and localization resources:

```csharp
using FluentAnnotationsValidator.Extensions;
using System.Globalization;

services.UseFluentAnnotations()
    .For<LoginDto>()
        .WithCulture(CultureInfo.GetCultureInfo("fr-FR"))
        .WithValidationResource<ValidationMessages>() // Scoped .resx lookup
    .Build();
```

Or inject conditional validation logic with localized error fallback:

```csharp
services.UseFluentAnnotations()
    .For<LoginDto>()
        .WithCulture(CultureInfo.GetCultureInfo("fr-FR"))
        .WithValidationResource<ValidationMessages>()
        .When(x => x.Password, dto => string.IsNullOrEmpty(dto.Password))
            .Localized("Password_Required")            // Looks up ValidationMessages.Password_Required
            .UseFallbackMessage("Mot de passe requis.") // Fallback if resource key is missing
    .Build();
```

---

### Highlights

- `WithCulture(...)` binds `CultureInfo` globally across rules for this type.
- `WithValidationResource<T>()` uses `.resx`-backed messages with safe fallback.
- `When(...)` applies targeted conditions with fluent chaining for per-property logic.
- `Localized(...)` uses string keys like `"Email_Required"`, typically mapped to `ValidationMessages` resource entries.

---

## Documentation

See [configuration guide](https://github.com/bigabdoul/fluent-annotations-validator/blob/main/docs/configuration/fluent.md) for advanced usage.

## Contribute

Open to feedback, extensions, and collaboration - shape validation ergonomics for developers worldwide.
