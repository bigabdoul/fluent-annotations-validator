## 🛠 Contributor Onboarding (v2.0+)

## Getting Started with FluentAnnotationsValidator

Welcome, validator enthusiast! 🎉 This project reimagines .NET validation with a fluent, localized, and highly extensible approach.

### 🔧 Prerequisites
- [.NET SDK 8.0+](https://dotnet.microsoft.com/)
- Familiarity with data annotations or FluentValidation-style APIs
- Basic understanding of DI and attribute-based programming

### 🧩 Repo Structure

| Folder / File                     | Purpose                                  |
|----------------------------------|-------------------------------------------|
| `src/`                           | Core library implementation               |
| `src/Configuration/`            | Fluent DSL and rule registration logic    |
| `src/Internals/Reflection/`     | Attribute introspection and caching       |
| `src/Messages/`                 | Message resolution and fallbacks          |
| `tests/`                         | NUnit test suite with realistic scenarios |

### 🧪 Running Tests

```bash
dotnet test
```

Look out for:
- `ValidationTypeConfiguratorTests.cs` — API surface behavior
- `DIRegistrationTests.cs` — scoped registration + resolution
- `Resolver_CultureTests.cs` — localization fallback strategies

### 🌱 Contributing

You can help with:
- Extending `ValidationConfigurator` with reusable patterns
- Improving localization sources or fallback behavior
- Refactoring and onboarding utilities
- Benchmarking attribute vs fluent performance

> **Start with a draft PR** — we love discussing architecture out loud.

---

## 🔄 Upcoming Work: `ValidationConfigurator.ForEach(...)`

To validate collection items fluently, propose this shape:

```csharp
configurator.ForEach(x => x.Items)
    .AddRule(item => item.Quantity)
    .When(item => item.IsActive)
    .WithMessage("Item must have quantity when active.");
```

### 💡 Under the Hood

- Wraps `MemberInfo` for indexed access
- Iterates with `GetValue(obj)` over `IEnumerable<T>`
- Internally calls `Validate(item)` with scoped context
- Aggregates child validation errors with indexed paths (`Items[2].Quantity`)
