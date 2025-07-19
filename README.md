# FluentAnnotationsValidator

[![NuGet - FluentAnnotationsValidator](https://img.shields.io/nuget/v/FluentAnnotationsValidator.svg)](https://www.nuget.org/packages/FluentAnnotationsValidator)
[![NuGet - FluentAnnotationsValidator.AspNetCore](https://img.shields.io/nuget/v/FluentAnnotationsValidator.AspNetCore.svg)](https://www.nuget.org/packages/FluentAnnotationsValidator.AspNetCore)
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
| FluentAnnotationsValidator | 1.0.6 | `dotnet add package FluentAnnotationsValidator` |
| FluentAnnotationsValidator.AspNetCore | 1.0.6 | `dotnet add package FluentAnnotationsValidator.AspNetCore` |

---

## 🚀 Quickstart

### 1. Register validators

```csharp
builder.Services.AddFluentAnnotationsValidators();
```

### 2. Annotate your DTO

```csharp
[ValidationResource(typeof(ValidationMessages))]
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

## 📚 Project Layout

```
src/
├── FluentAnnotationsValidator/
│   ├── DataAnnotationsValidator.cs
│   ├── ValidationMetadataCache.cs
│   ├── PropertyValidationInfo.cs
│   ├── ValidationMessageResolver.cs
│   └── ValidationResourceAttribute.cs
├── FluentAnnotationsValidator.AspNetCore/
│   └── ServiceCollectionExtensions.cs
tests/
├── FluentAnnotationsValidator.Tests/
│   ├── Models/
│   ├── Validators/
│   └── Resources/
```

---

## 📘 Documentation

Detailed guides, examples, and diagrams are available in the [documentation folder](docs/index.md):

- Architecture internals
- Localization strategies
- Extensibility patterns
- Validation flow breakdown

---

## 📄 License

Licensed under the MIT License.

---

## 🤝 Contributing

Have an idea for an extension point, message strategy, or improved experience? PRs and issues welcome. This validator is designed to be extensible, teachable, and fun to debug.
