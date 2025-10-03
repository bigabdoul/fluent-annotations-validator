# FluentAnnotationsValidator

A fluent, type-safe validation engine for .NET that transforms `[ValidationAttribute]` annotations into runtime validation logic. Designed for ergonomic configuration, conditional logic, and culture-aware localization.

---

### Version 2.0.0-rc.1.0.0: New Modular Foundation



---

#### New API and Behavior

* **`Rule(...)`**: This method is now overloaded. It takes a property 
expression and an optional `RuleDefinitionBehavior` enum. By default, 
it replaces all previously registered rules for that property before 
adding the new one. The method returns a reference to the 
`FluentTypeValidator<T>` instance, allowing you to configure 
the rule further.

* **`RuleFor(...)`**: This new method takes a property expression and 
returns a reference to a new, property-specific builder 
(`IValidationRuleBuilder<T, TProp>`). It does **not** override existing 
rules for the property. If you want to override pre-registered rules,
call the `RemoveRulesFor<TProp>(Expression<Func<T, TProp>>)` method.
This new builder enables powerful, type-safe chaining of 
validation methods and conditional logic.

---

#### New Features

* **Conditional Validation (`When`/`Otherwise`)**: The `RuleFor(...)` builder introduces a robust way to implement conditional logic.
    * **`When(condition, configureRules)`**: This method allows you to group multiple validation rules that will only be evaluated if the provided `condition` is true.
    * **`Otherwise(configureRules)`**: This method provides a corresponding set of rules that will be evaluated if the `When` condition is false, creating a clear `if/else` validation flow.

* **Custom Validation (`Must`)**: The new **`Must(predicate)`** method is a key addition to the `IValidationRuleBuilder<T, TProp>`. It allows developers to define custom validation logic using a predicate (`Func<TProp, bool>`) that is executed on the property's value. This method is fully integrated into the fluent chaining, enabling complex validation rules that are not possible with standard attributes.

---

## 🌟 What's New in v2.0.0-rc.1.0.0

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
using FluentAnnotationsValidator;

services.AddFluentAnnotations();
```

### Advanced Setup

Using either:

1. `AddFluentAnnotationsValidators(...)`:

```csharp
using var config = services.AddFluentAnnotationsValidators(typeof(LoginDto))
    .UseFluentAnnotations()
    .WithCulture(CultureInfo.GetCultureInfo("fr-FR"))
    .WithValidationResource<ValidationMessages>()
    .For<LoginDto>();

// Configure rules
// config.RuleFor(...);
```

2. `AddFluentAnnotations(...)` with localizer factory:

```csharp
using FluentAnnotationsValidator.Runtime;

services.AddFluentAnnotations(mew ConfigurationOptions
{
    LocalizerFactory: factory =>
    {
        // common culture and resource type for all validation attributes
        return new StringLocalizerFactoryResult
        (
            SharedResourceType: typeof(ValidationMessages),
            SharedCulture: CultureInfo.GetCultureInfo("fr-FR")
        );
    }
});
```

3. `AddFluentAnnotations(...)` with scoped shared resource type and culture:
```csharp
services.AddFluentAnnotations(new()
{
    ConfigureValidatorRoot = builder =>
    {
        // Conditional localized rule configuration for the 
        // German culture and resource type scoped to LoginDto.
        builder.For<LoginDto>()
            .WithCulture(CultureInfo.GetCultureInfo("de-DE"))
            .WithValidationResource<AuthenticationMessages>()
            .Rule(x => x.Email, RuleDefinitionBehavior.Preserve)
            .Required()
            .EmailAddress()
            .When(x => x.LangCode == 'DE')
        .Build();
    },
    // common French culture and resource type for all validation rules
    LocalizerFactory = factory => new
    (
        typeof(ValidationMessages),
        CultureInfo.GetCultureInfo("fr-FR")
    )
});
```
---

## Installation

```bash
dotnet add package FluentAnnotationsValidator --version 2.0.0-rc.1.0.0
```

---

## Key Concepts

| Concept                       | Description                                                                        |
|-------------------------------|-------------------------------------------------------------------------------------|
| `ValidationRuleGroupRegistry` | Registry of validation rules discovered via scanning or configuration               |
| `FluentAnnotationsBuilder`    | Configuration anchor: links DI services + options                                   |
| `FluentTypeValidatorRoot`     | Fluent DSL to configure conditional logic, culture, and resource resolution         |
| `IValidationMessageResolver`  | Pluggable fallback resolution for localized messages                                |
| `FluentValidator<T>`          | Runtime validator that hydrates validation rules from metadata and rules registry   |

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
