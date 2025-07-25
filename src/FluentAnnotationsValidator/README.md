# FluentAnnotationsValidator

A fluent, type-safe validation engine for .NET that transforms `[ValidationAttribute]` annotations into runtime validation logic. Designed for ergonomic configuration, conditional logic, and culture-aware localization.

---

## 🌟 What's New in v2.0.0-preview1

> FluentAnnotationsValidator v2.0.0-preview1 is a fresh rewrite. All legacy APIs from v1.x have been removed.

- Multi-attribute validation per property
- DSL-based configuration via `ValidationConfigurator`
- Conditional rules with `.When(...)`, `.Localized(...)`, `.UseFallbackMessage(...)`
- Convention-based registration from scanned assemblies
- Pluggable `IValidationMessageResolver`
- Scoped culture + resource binding per type
- Legacy support removed — clean slate architecture

To use the legacy version, pin to [v1.2.2](https://www.nuget.org/packages/FluentAnnotationsValidator/1.2.2).

---

## Quickstart

### Basic Setup

Using `AddFluentAnnotations()`:

```csharp
using FluentAnnotationsValidator.Extensions;

services.AddFluentAnnotations();
```

### Advanced Setup

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
---

## Installation

```bash
dotnet add package FluentAnnotationsValidator --version 2.0.0-preview1
```

---

## Key Concepts

| Concept                      | Description                                                                             |
|-----------------------------|-----------------------------------------------------------------------------------------|
| `ValidationBehaviorOptions` | Registry of validation rules discovered via scanning or configuration                  |
| `FluentAnnotationsBuilder`  | Configuration anchor: links DI services + options                                     |
| `ValidationConfigurator`    | Fluent DSL to configure conditional logic, culture, and resource resolution           |
| `IValidationMessageResolver`| Pluggable fallback resolution for localized messages                                   |
| `DataAnnotationsValidator<T>` | Runtime validator that hydrates validation rules from metadata and rules registry     |

---

## Test Coverage

- ✅ `[Required]`, `[EmailAddress]`, `[MinLength]`, `[Range]`, `[StringLength]`
- ✅ `.resx` and static resource support
- ✅ Record constructor annotations
- ✅ Upfront rule hydration + conditional overrides

---

## Learn More

- [GitHub Repository](https://github.com/bigabdoul/fluent-annotations-validator)
- [Documentation](https://github.com/bigabdoul/fluent-annotations-validator/blob/main/docs/configuration/fluent.md)

---

## Contribute

Help shape validation ergonomics for developers worldwide. Open to extensions, diagnostics, and new DSL patterns — bring your ideas!
