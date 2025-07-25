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

#### Basic Setup

Using `AddFluentAnnotations()`:

```csharp
using FluentAnnotationsValidator.Extensions;

services.AddFluentAnnotations();
```

#### Advanced Setup

Using either:

1. `AddFluentAnnotationsValidators(...)`:

```csharp
services.AddFluentAnnotationsValidators(typeof(LoginDto))
    .UseFluentAnnotations()
    .For<LoginDto>()
        .WithCulture(CultureInfo.GetCultureInfo("fr-FR"))
        .WithValidationResource<ValidationMessages>()
    .Build();
```

2. `AddFluentAnnotations(...)` with common behavior options configuration:

```csharp
services.AddFluentAnnotations(
    configureBehavior: options =>
    {
        // common culture and resource type for all validation attributes
        options.CommonCulture = CultureInfo.GetCultureInfo("fr-FR");
        options.CommonResourceType = typeof(ValidationMessages);
    }
);
```

3. `AddFluentAnnotations(...)` with scoped and common culture and resource types:
```csharp
services.AddFluentAnnotations(
    builder =>
        // Conditional Localization rule for German 
        // culture and resource type scoped to LoginDto
        builder.For<LoginDto>()
            .When(x => x.LangCode == 'DE')
            .WithCulture(CultureInfo.GetCultureInfo("de-DE"))
            .WithValidationResource<AuthenticationMessages>()
        .Build(),
    configureBehavior: options =>
    {
        // common French culture and resource type for all validation rules
        options.CommonCulture = CultureInfo.GetCultureInfo("fr-FR");
        options.CommonResourceType = typeof(ValidationMessages);
    }
);
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
- ✅ All legacy use-case tests are passing
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
│   ├── Internals/Reflection // Metadata cache
│   ├── Messages/           // Fallback + formatting logic
│   ├── Metadata/           // Resource marker attribute
│   ├── Results/            // Validation results
│   └── Runtime/Validators  // Validator factories

tests/
├── FluentAnnotationsValidator.Tests/
│   ├── Assertions/
│   ├── Configuration/
│   ├── Messages/
│   ├   └── Resolutions/
│   ├── Models/
│   ├── Resources/
│   ├── Results/
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
