# FluentAnnotationsValidator

[![NuGet - FluentAnnotationsValidator](https://img.shields.io/nuget/v/FluentAnnotationsValidator.svg)](https://www.nuget.org/packages/FluentAnnotationsValidator)
[![Build Status](https://github.com/bigabdoul/fluent-annotations-validator/actions/workflows/nuget-publish.yml/badge.svg)](https://github.com/bigabdoul/fluent-annotations-validator/actions)
[![Source Link](https://img.shields.io/badge/SourceLink-enabled-brightgreen)](https://github.com/dotnet/sourcelink)

A lightweight, dynamic bridge between `System.ComponentModel.DataAnnotations` and FluentValidation.

Supports localized error messages, DI registration, convention-based resolution, ASP.NET Core integration — and ships with full Source Link and step-through symbol support.

---

## ✨ Purpose

`FluentAnnotationsValidator` is a reflection-powered adapter that converts standard `[ValidationAttribute]` annotations into fluent validation rules at runtime. It supports localized error messaging, DI registration, performance caching, and debugging, making it a drop-in enhancement for any .NET API or ASP.NET Core backend.

---

## 🧠 Key Features

- Converts `[Required]`, `[EmailAddress]`, `[MinLength]`, `[StringLength]`, `[Range]`, and more to FluentValidation rules
- Resolves localized error messages from `.resx` or static resource classes
- Supports conventional message keys (`Property_Attribute`) and explicit `ErrorMessageResourceName`
- Seamless registration via DI (`IValidator<T>`)
- High-performance validation with caching and no boilerplate
- Compatible with Minimal APIs, MVC, Blazor, and Web APIs
- Includes Source Link metadata for step-through debugging
- Published with deterministic builds and `.snupkg` symbols

---

## 📦 Installation

Add via NuGet:

| Package | Version | Install |
|--------|---------|---------|
| FluentAnnotationsValidator | 1.1.0 | `dotnet add package FluentAnnotationsValidator` |

---

## 🚀 Quickstart

### 1. Register validators

```csharp
using FluentAnnotationsValidator.Extensions;

builder.Services.AddFluentAnnotationsValidators();
```

### Fluent Validation Configuration

Define conditional rules per property, model, and context:

```csharp
services.UseFluentAnnotations()
    .For<LoginDto>()
        .When(x => x.Email, dto => dto.Role != null && dto.Role != "Admin")
            .WithMessage("Non-admins must provide a valid email.")
            .WithKey("Email.NonAdminRequired")
            .Localized("NonAdmin_Email_Required")
        .Except(x => x.Role)
        .AlwaysValidate(x => x.Password)
    .For<RegistrationDto>()
        .When(x => x.Age, dto => dto.Age >= 18)
    .Build();
```

### 2. Annotate your DTO

```csharp
using FluentAnnotationsValidator.Metadata;

[ValidationResource(typeof(ValidationMessages))]
public class RegistrationDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = default!;

    public string? Role { get; set; }
}
```

### 3. Define localized messages

```csharp
public static class ValidationMessages
{
    public const string Email_Required = "Email is required.";
    public const string Email_EmailAddress = "Email format is invalid.";
    public const string Password_Required = "Password is required.";
    public const string Password_MinLength = "Password must be at least {0} characters.";
}
```

### 4. Validate in endpoint

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

Included in `FluentAnnotationsValidator.Tests`:

- Unit tests for all supported `ValidationAttribute` types
- Localized error resolution using `.resx` and static resource classes
- Edge cases like invalid formats, missing values, and multiple violations
- Deterministic build verification and CI workflow coverage

---

## 🗂️ Solution Structure

| Project | Description |
|--------|-------------|
| `FluentAnnotationsValidator` | Core library for fluent, annotation-driven validation |
| `FluentAnnotationsValidator.Tests` | Unified test suite for all validation logic and fluent configuration |

### 📚 Project Layout

```
src/
├── FluentAnnotationsValidator/
│   ├── Abstractions/
│   │   ├── IValidationConfigurator.cs
│   │   ├── IValidationMessageResolver<T>.cs
│   │   └── IValidationTypeConfigurator.cs
│   ├── Configuration/
│   │   ├── ValidationBehaviorOptions.cs
│   │   └── ValidationTypeConfigurator<T>.cs
│   ├── Extensions/
│   │   ├── ValidationBehaviorOptionsExtensions.cs
│   │   └── ValidatorServiceCollectionExtensions.cs
│   ├── Internal/
│   │   └── Reflection/
│   │       ├── PropertyValidationInfo.cs
│   │       └── ValidationMetadataCache.cs
│   ├── Messages/
│   │   └── ValidationMessageResolver.cs
│   ├── Runtime/
│   │   └── Validators/
│   │       └── DataAnnotationsValidator<T>.cs
│   └── Metadata/
│       └── ValidationResourceAttribute.cs

tests/
├── FluentAnnotationsValidator.Tests/
│   ├── Assertions/
│   │   └── ValidationAssertions.cs
│   ├── Configuration/
│   │   └── ValidationTypeConfiguratorTests.cs
│   ├── Models/
│   │   ├── TestLoginDto.cs
│   │   └── TestRegistrationDto.cs
│   ├── Resources/
│   │   └── ValidationMessages.cs
│   ├── Validators/
│   │   └── RegistrationValidatorTests.cs
│   ├── DIRegistrationTests.cs
|   └── TestHelpers.cs
```

---

## 📘 Documentation

Detailed guides, examples, and diagrams are available in the [documentation folder](docs/index.md):

- [Architecture internals](docs/architecture.md)
- [Localization strategies](docs/localization.md)
- [Customization & Extensibility](docs/customization.md)
- [Validation flow breakdown](docs/validation-flow.md)
- [Fluent validation configuration](docs/configuration/fluent.md)

Supports:
- Strongly typed property access
- Conditional logic via lambdas
- Localized and custom error messages
- Composable, discoverable, ergonomic configuration

Full guide in [`docs/configuration/fluent.md`](docs/configuration/fluent.md)

---

## 📄 License

Licensed under the MIT License.

---

## 🤝 Contributing

Have an idea for an extension point, message strategy, or improved experience? PRs and issues welcome. This validator is designed to be extensible, teachable, and fun to debug.
