## FluentAnnotationsValidator.Core

This package provides the core abstractions, fluent APIs, and rule composition interfaces for building expressive validation pipelines in .NET.

### Features

- `IValidationRuleBuilder<T, TProp>` – Fluent rule builder interface
- Extension methods for composing validation logic
- Support for chaining, conditional rules, and metadata-driven validation
- Designed for integration with custom validators or runtime engines

### Installation

```bash
dotnet add package FluentAnnotationsValidator.Core
```

### Usage

```csharp
builder.RuleFor(x => x.Quantity)
       .Minimum(1)
       .Maximum(100)
       .WithMessage("Quantity must be between 1 and 100.");
```

### Related Packages

- [FluentAnnotationsValidator.Annotations](https://www.nuget.org/packages/FluentAnnotationsValidator.Annotations)
- [FluentAnnotationsValidator.Runtime](https://www.nuget.org/packages/FluentAnnotationsValidator.Runtime)

### License

MIT
