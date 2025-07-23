# FluentAnnotationsValidator

[![NuGet - FluentAnnotationsValidator](https://img.shields.io/nuget/v/FluentAnnotationsValidator.svg)](https://www.nuget.org/packages/FluentAnnotationsValidator)
[![Build Status](https://github.com/bigabdoul/fluent-annotations-validator/actions/workflows/nuget-publish.yml/badge.svg)](https://github.com/bigabdoul/fluent-annotations-validator/actions)
[![Source Link](https://img.shields.io/badge/SourceLink-enabled-brightgreen)](https://github.com/dotnet/sourcelink)

A lightweight, dynamic bridge between `System.ComponentModel.DataAnnotations` and FluentValidation. Supports localized error messages, DI registration, convention-based resolution, ASP.NET Core integration — and ships with full Source Link and step-through symbol support.

---

## ✨ Purpose

FluentAnnotationsValidator is a reflection-powered adapter that converts standard `[ValidationAttribute]` annotations into fluent validation rules at runtime. It handles localized messaging, performance caching, debug support, and DI registration — making it a seamless enhancement for any .NET API or ASP.NET Core backend.

---

## 🧠 Key Features

- Converts `[Required]`, `[EmailAddress]`, `[Range]`, `[MinLength]`, `[StringLength]`, etc. into runtime FluentValidation rules
- Localized error messages via `.resx` or static resource classes
- Supports conventional message keys (`Property_Attribute`) and explicit `ErrorMessageResourceName`
- Conditional rules with lambda-based validation and fallback messages
- Culture-aware formatting for scalars and arrays (e.g., `{0}`, `{1}`)
- DI integration (`IValidator<T>`) for use in endpoints and controllers
- High-performance with metadata caching, no boilerplate
- Fluent DSL configuration with discoverable chaining
- Source Link + deterministic builds + `.snupkg` symbol publishing

---

## 📦 Installation

Install via NuGet:

| Package | Version | Command |
|--------|---------|---------|
| FluentAnnotationsValidator | 1.2.2 | `dotnet add package FluentAnnotationsValidator` |

---

> ⚠️ **Heads-up:** Current version supports one rule per property.
>
> If a property has multiple `[ValidationAttribute]`s, only the last error will be emitted.
>
> This will be resolved in the upcoming **v2.0.0** with full multi-message support.

---

## 🚀 Quickstart

### 1. DTO Annotations

```csharp
using FluentAnnotationsValidator.Metadata;

[ValidationResource(typeof(ValidationMessages))] // optional; can be set via .WithValidationResource<T>()
public class RegistrationDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = default!;
}
```

### 2. Localized Messages (`.resx` or static class)

```csharp
public static class ValidationMessages
{
    public const string Email_Required = "Email is required.";
    public const string Email_EmailAddress = "Email format is invalid.";
    public const string Password_Required = "Password is required.";
    public const string Password_MinLength = "Password must be at least {0} characters.";
}
```

### 3. Configuration

#### Basic Registration

```csharp
using FluentAnnotationsValidator.Extensions;
using System.Globalization;

// Initialization required to auto-discovery of [ValidationAttribute]
services.AddFluentAnnotationsValidators();

// or (for better performance, load only assemblies of targeted types):
// services.AddFluentAnnotationsValidators(typeof(RegistrationDto));

services.UseFluentAnnotations()
    .For<RegistrationDto>()
        .WithCulture(CultureInfo.GetCultureInfo("fr-FR"))
        .WithValidationResource<ValidationMessages>()
    .Build();
```

#### Conditional Fallback Logic

```csharp
services.UseFluentAnnotations()
    .For<RegistrationDto>()
        .WithCulture(CultureInfo.GetCultureInfo("fr-FR"))
        .WithValidationResource<ValidationMessages>()
        .When(x => x.Password, dto => string.IsNullOrEmpty(dto.Password))
            .Localized("Password_Required")
            .UseFallbackMessage("Mot de passe requis.")
    .Build();
```

#### Global Configuration (Coming soon)

```csharp
services.UseFluentAnnotations()
    .ForAll() // Or .ForAll(typeof(DtoA), typeof(DtoB))
        .WithCulture(CultureInfo.GetCultureInfo("fr-FR"))
        .WithValidationResource<ValidationMessages>()
    .Build();
```

### 4. Runtime Validation

```csharp
app.MapPost("/register", async (RegistrationDto dto, IValidator<RegistrationDto> validator) =>
{
    var result = await validator.ValidateAsync(dto);
    if (!result.IsValid)
        return Results.BadRequest(result.Errors);

    // Proceed with registration
});
```

---

## 🧪 Testing

Inside `FluentAnnotationsValidator.Tests`:

- ✅ Unit tests for all `[ValidationAttribute]` types
- ✅ Resolution of localized strings via `.resx` and static resources
- ✅ Edge cases like multiple violations and fallback messages
- ✅ Deterministic build and workflow verification

---

## 🗂️ Solution Structure

| Project | Purpose |
|--------|---------|
| `FluentAnnotationsValidator` | Core validation engine, DSL, and culture-aware formatting |
| `FluentAnnotationsValidator.Tests` | Complete test suite covering configuration, resolution, formatting |

### 📁 Project Layout

```
src/
├── FluentAnnotationsValidator/
│   ├── Abstractions/       // Interfaces for config, resolvers
│   ├── Configuration/      // DSL surface and options
│   ├── Extensions/         // Service registration
│   ├── Internal/Reflection // Metadata cache
│   ├── Messages/           // Fallback + formatting logic
│   ├── Runtime/Validators  // Validator factories
│   └── Metadata/           // Resource marker attribute

tests/
├── FluentAnnotationsValidator.Tests/
│   ├── Assertions/
│   ├── Configuration/
│   ├── Models/
│   ├── Resources/
│   ├── Validators/
│   ├── DIRegistrationTests.cs
│   └── TestHelpers.cs
```

---

## 📘 Documentation

Explore advanced flows in the [`docs`](docs/index.md):

- [Architecture](docs/architecture.md)
- [Localization](docs/localization.md)
- [Extensibility](docs/customization.md)
- [Validation flow](docs/validation-flow.md)
- [Fluent DSL config](docs/configuration/fluent.md)

---

## 📄 License

Licensed under the MIT License.

---

## 🤝 Contributing

Ideas welcome — from resource strategies to DSL ergonomics. Submit pull requests, file issues, and join the mission to make validation intuitive and multilingual.
