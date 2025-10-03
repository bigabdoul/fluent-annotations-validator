## FluentAnnotationsValidator.Annotations

This package provides a curated set of custom validation attributes designed for fluent rule composition in .NET applications. It is part of the modular FluentAnnotationsValidator ecosystem.

### Features

- `MinimumAttribute`, `MaximumAttribute` – Range validation for numeric and enum types
- `CountPropertyAttribute` – Marks custom properties as count sources for collection validation
- Enum-aware and culture-aware comparison logic
- Inclusive and exclusive comparison modes
- XML-documented for IntelliSense and contributor clarity

### Installation

```bash
dotnet add package FluentAnnotationsValidator.Annotations
```

### Usage

```csharp
public class Product
{
    [Minimum(1)]
    public int Quantity { get; set; }

    [Maximum(PriorityLevel.High)]
    public PriorityLevel Priority { get; set; }
}
```

### Related Packages

- [FluentAnnotationsValidator.Core](https://www.nuget.org/packages/FluentAnnotationsValidator.Core)
- [FluentAnnotationsValidator.Runtime](https://www.nuget.org/packages/FluentAnnotationsValidator.Runtime)

### License

MIT
