---
title: Fluent Configuration API
breadcrumb: [FluentAnnotationsValidator](../../README.md) > [Documentation](../index.md) > [Fluent Configuration API](fluent.md)
version: v1.1.0
---

# Fluent Configuration API

The fluent DSL (Domain-Specific Language) allows you to configure conditional validation per DTO and property using expressive, type-safe chaining.

## Example

```csharp
// targetAssembliesTypes: optional, helps boost performance 
// at startup by scanning only assemblies of targeted types;

// here, we provide two types (if they're in different asm);
// if they're in the same asm, provide only one type.
services.AddFluentAnnotationsValidators(targetAssembliesTypes: [typeof(LoginDto), typeof(RegistrationDto)]);

services.UseFluentAnnotations()
    .For<LoginDto>()
        .When(x => x.Email, dto => dto.Role != "Admin")
            .WithMessage("Non-admins must provide a valid email.")
            .WithKey("Email.NonAdminRequired")
            .Localized("NonAdmin_Email_Required")
        .Except(x => x.Role)
        .AlwaysValidate(x => x.Password)
    .For<RegistrationDto>()
        .When(x => x.Age, dto => dto.Age >= 18)
    .Build();
```

## Available Methods

| Method               | Description                                                  |
|----------------------|--------------------------------------------------------------|
| `For<T>()`           | Start configuring a specific model type                      |
| `When(...)`          | Register a conditional rule for a property                   |
| `And(...)`           | Alias for `.When(...)` on additional properties              |
| `Except(...)`        | Skip validation for a specific property                      |
| `AlwaysValidate(...)`| Force unconditional validation                               |
| `WithMessage(...)`   | Set a custom error message                                   |
| `WithKey(...)`       | Define a message key for diagnostics or resolvers            |
| `Localized(...)`     | Provide a resource key for localization                      |
| `Build()`            | Finalize and flush all configuration to the DI container     |

## Runtime Behavior

- Each condition is stored in `ValidationBehaviorOptions.PropertyConditions`
- Metadata is forwarded to the message resolver
- Fluent chaining supports deferred rule registration and late-bound overrides

## ✅ Tips

- Chain `.WithMessage(...)` etc. before `.For<T>()` or `.Build()` to apply metadata
- Use lambda expressions for type safety and auto-completion
