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
services.UseFluentAnnotations()
    .For<LoginDto>()
        .WithValidationResource<ValidationMessages>()
        // or:
        // .WithValidationResource(typeof(ValidationMessages))
        .When(x => x.Email, dto => string.IsNullOrEmpty(dto.Email))
            .Localized("Email_Required")
        .Build();

// Runtime lookup
var options = new ValidationBehaviorOptions();

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
        .WithValidationResource<ValidationMessages>()
        .When(x => x.Password, dto => string.IsNullOrEmpty(dto.Password))
            .WithMessage("Password is missing.")
            .Localized("Password_Required")
        .Build();
```

This eliminates the need for `[ValidationResource(...)]` on your DTO — making localization more discoverable and intentional from configuration code.
