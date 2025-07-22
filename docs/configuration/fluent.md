---
title: Fluent Configuration API
breadcrumb: FluentAnnotationsValidator > Documentation > Fluent Configuration API
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

## 🧩 Accessing Rules by Property

After calling `.Build()`, your conditional rules are stored in the runtime pipeline and can be inspected or retrieved dynamically.

Here’s your updated **API documentation** section for the rule lookup logic — incorporating the full `ValidationBehaviorOptions` interface with consistent formatting, descriptive guidance, and a usage example featuring `.WithValidationResource<T>()` 💡📘

---

## 🔧 Rule Lookup API

Once rules are registered via `.For<T>()...Build()`, you can inspect, retrieve, or verify rule presence through the `ValidationBehaviorOptions` container.

### 🔍 Lookup Methods

| Method | Description |
|--------|-------------|
| `Set(Type modelType, string propertyName, rule)` | Registers or replaces a rule using type + property string |
| `Set<T>(Expression<Func<T, string?>> property, rule)` | Registers a rule using a strongly typed lambda |
| `Get(Type modelType, string propertyName)` | Retrieves a rule or throws if missing |
| `Get<T>(Expression<Func<T, string?>> property)` | Retrieves via lambda or throws if missing |
| `TryGet(Type modelType, string propertyName, out rule)` | Attempts lookup by type + string name |
| `TryGet<T>(Expression<Func<T, string?>> property, out rule)` | Attempts lookup via lambda expression |
| `ContainsKey(Type modelType, string propertyName)` | Checks for existence using raw types and strings |
| `ContainsKey<T>(Expression<Func<T, string?>> property)` | Checks existence via strongly typed lambda |

All lambda-based overloads use a compiler-safe approach via `Expression<Func<T, ...>>`, promoting refactorable, discoverable DX.

---

### 📘 Example with Resource Lookup

```csharp
// Always make sure to scan target assemblies on application start up.
services.AddFluentAnnotationsValidators(typeof(LoginDto));

services.UseFluentAnnotations()
    .For<LoginDto>()
        .WithValidationResource<ValidationMessages>()
        // or:
        // .WithValidationResource(typeof(ValidationMessages))
        .When(x => x.Email, dto => string.IsNullOrEmpty(dto.Email))
            .Localized("Email_Required")
        .Build();

// Runtime lookup
var options = services.BuildServiceProvider().GetRequiredService<IOptions<ValidationBehaviorOptions>>().Value;

if (options.TryGet<LoginDto>(x => x.Email, out var rule))
{
    Console.WriteLine($"Rule key: {rule.ResourceKey}");
    Console.WriteLine($"Rule type: {rule.ResourceType?.Name}");
}
```

This example demonstrates fluent configuration with a scoped resource type, and rule inspection via the strongly typed lambda API.

---

## 🌍 Localization via Resource Scoping

You can declaratively set the resource type for all `.Localized(...)` keys within a fluent configuration chain using `.WithValidationResource<T>()`.

### 📘 Example

```csharp
services.UseFluentAnnotations()
    .For<LoginDto>()
        .WithValidationResource(typeof(ValidationMessages))
        .When(x => x.Password, dto => string.IsNullOrEmpty(dto.Password))
            .WithMessage("Password is missing.")
            .Localized("Password_Required")
        .Build();
```

This eliminates the need for `[ValidationResource(...)]` on your DTO — making localization more discoverable and intentional from configuration code.
