# FluentAnnotationsValidator

A fluent, type-safe validation engine for .NET that transforms `[ValidationAttribute]` annotations into runtime validation logic. Supports conditional rules, culture-scoped messages, and discoverable configuration via DSL.

---

## Features

- Automatic validation from `[Required]`, `[Range]`, `[EmailAddress]`, etc.
- Culture-aware localization from `.resx` or static resource classes
- Conditional logic per property via `.When(...)`, `.Localized(...)`, `.UseFallbackMessage(...)`
- Implicit rule generation based on global config (`.WithCulture(...)`, `.WithValidationResource(...)`)
- Smooth integration with ASP.NET and `IValidator<T>`
- DSL configuration: target types, cultures, resource mapping, conventions

## Quick Setup

### Fluent Validation Configuration

Start with automatic rule generation via `[ValidationAttribute]`, then layer culture and localization resources:

```csharp
using FluentAnnotationsValidator.Extensions;
using System.Globalization;

services.AddFluentAnnotationsValidators(typeof(LoginDto))
    .UseFluentAnnotations()
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
            .Localized("Password_Required") // Looks up ValidationMessages.Password_Required
            .UseFallbackMessage("Mot de passe requis.") // Fallback if resource key is missing
    .Build();
```

> ⚠️ **Heads-up:** Current version supports only **one rule per property**.  
> If a property has multiple `[ValidationAttribute]`s, only the last one will be processed.  
> This will be resolved in **v2.0.0** with full multi-attribute resolution.

---

## v1.2.2 Highlights

- Fixed implicit rule synthesis for attributes lacking `.When(...)`
- Respects global culture and resource bindings during fallback
- Improved configurator registry architecture for future rule aggregation
- Warning added for multi-attribute scenarios pending v2.0.0 release

---

## Installation

```bash
dotnet add package FluentAnnotationsValidator --version 1.2.2
```

---

## Test Coverage

- ✅ `[Required]`, `[EmailAddress]`, `[MinLength]`, `[Range]` support
- ✅ Localized message binding from culture+resource
- ✅ Fallback behavior for `.Build()` without manual rule definitions

---

## Learn More

- [GitHub Repository](https://github.com/bigabdoul/fluent-annotations-validator)
- [NuGet Package](https://www.nuget.org/packages/FluentAnnotationsValidator/1.2.2)

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
