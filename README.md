# FluentAnnotationsValidator

[![NuGet - FluentAnnotationsValidator](https://img.shields.io/nuget/v/FluentAnnotationsValidator.svg)](https://www.nuget.org/packages/FluentAnnotationsValidator)
[![NuGet - FluentAnnotationsValidator.Annotations](https://img.shields.io/nuget/v/FluentAnnotationsValidator.Annotations.svg)](https://www.nuget.org/packages/FluentAnnotationsValidator.Annotations)
[![NuGet - FluentAnnotationsValidator.Runtime](https://img.shields.io/nuget/v/FluentAnnotationsValidator.Runtime.svg)](https://www.nuget.org/packages/FluentAnnotationsValidator.Runtime)
[![NuGet - FluentAnnotationsValidator.Core](https://img.shields.io/nuget/v/FluentAnnotationsValidator.Core.svg)](https://www.nuget.org/packages/FluentAnnotationsValidator.Core)
[![NuGet - FluentAnnotationsValidator.AspNetCore](https://img.shields.io/nuget/v/FluentAnnotationsValidator.AspNetCore.svg)](https://www.nuget.org/packages/FluentAnnotationsValidator.AspNetCore)
[![NuGet Publish](https://github.com/bigabdoul/fluent-annotations-validator/actions/workflows/nuget-publish.yml/badge.svg)](https://github.com/bigabdoul/fluent-annotations-validator/actions)
[![Build](https://github.com/bigabdoul/fluent-annotations-validator/actions/workflows/build.yml/badge.svg)](https://github.com/bigabdoul/fluent-annotations-validator/actions/workflows/build.yml)
[![Test](https://github.com/bigabdoul/fluent-annotations-validator/actions/workflows/test.yml/badge.svg)](https://github.com/bigabdoul/fluent-annotations-validator/actions/workflows/test.yml)
[![GitHub Pages](https://github.com/bigabdoul/fluent-annotations-validator/actions/workflows/docfx.yml/badge.svg)](https://github.com/bigabdoul/fluent-annotations-validator/actions/workflows/docfx.yml)
[![Source Link](https://img.shields.io/badge/SourceLink-enabled-brightgreen)](https://github.com/dotnet/sourcelink)

**FluentAnnotationsValidator** is a modern, fluent validation library for .NET that simplifies the process of defining complex validation rules for your C# models. It combines the declarative clarity of `[ValidationAttribute]` annotations with the expressive power of a fluent API, enabling ergonomic, type-safe rule configuration.

Powered by reflection, FluentAnnotationsValidator dynamically transforms standard data annotations into runtime validation logic. The latest version introduces a native validation engine that replaces the dependency on `FluentValidation`, offering a cleaner, more intuitive, and fully integrated experience tailored for both attribute-based and fluent validation flows.

---

## Packages

| Project                                | Description                                      | NuGet Package |
|----------------------------------------|--------------------------------------------------|---------------|
| FluentAnnotationsValidator             | Main package representing the entry point        | `FluentAnnotationsValidator` |
| FluentAnnotationsValidator.Annotations | Declarative validation attributes                | `FluentAnnotationsValidator.Annotations` |
| FluentAnnotationsValidator.Runtime     | Runtime rule execution and diagnostics           | `FluentAnnotationsValidator.Runtime` |
| FluentAnnotationsValidator.Core        | Fluent API and rule composition engine           | `FluentAnnotationsValidator.Core` |
| FluentAnnotationsValidator.AspNetCore  | ASP.NET Core integration (filters, DI, middleware)| `FluentAnnotationsValidator.AspNetCore` |

-----

## Dependency Hierarchy

```plaintext
FluentAnnotationsValidator
├── FluentAnnotationsValidator.Annotations
│   └── FluentAnnotationsValidator.Core
├── FluentAnnotationsValidator.Runtime
│   ├── FluentAnnotationsValidator.Annotations
│   └── FluentAnnotationsValidator.Core
└── FluentAnnotationsValidator.Core

FluentAnnotationsValidator.AspNetCore
└── FluentAnnotationsValidator.Core

┌─────────────────────────────────────────┐
│     FluentAnnotationsValidator          │
│                                         │
│  ┌───────────────────────────────────┐  │
│  │ FluentAnnotationsValidator.       │  │
│  │        Annotations                │  │
│  │ ┌────────────────────────────┐    │  │
│  │ │ FluentAnnotationsValidator │    │  │
│  │ │          .Core             │    │  │
│  │ └────────────────────────────┘    │  │
│  └───────────────────────────────────┘  │
│                                         │
│  ┌───────────────────────────────────┐  │
│  │ FluentAnnotationsValidator.       │  │
│  │         Runtime                   │  │
│  │ ┌──────────────────────────────┐  │  │
│  │ │ FluentAnnotationsValidator   │  │  │
│  │ │          .Core               │  │  │
│  │ └──────────────────────────────┘  │  │
│  │ ┌──────────────────────────────┐  │  │
│  │ │ FluentAnnotationsValidator   │  │  │
│  │ │       .Annotations           │  │  │
│  │ └──────────────────────────────┘  │  │
│  └───────────────────────────────────┘  │
│                                         │
│  ┌──────────────────────────────────┐   │
│  │ FluentAnnotationsValidator.      │   │
│  │           Core                   │   │
│  └──────────────────────────────────┘   │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ FluentAnnotationsValidator.AspNetCore   │
│                                         │
│ ┌─────────────────────────────────────┐ │
│ │ FluentAnnotationsValidator.Core     │ │
│ └─────────────────────────────────────┘ │
└─────────────────────────────────────────┘

```

---

## `RuleForEach` for Collection Validation

The `RuleForEach` method is a new addition that enables you to apply validation rules to each item within a collection. This is particularly useful for validating complex nested models, as it allows you to define rules for properties inside a list or array.

When combined with `When` and `Otherwise`, `RuleForEach` becomes a powerful tool for creating highly conditional validation logic on collections. The inner rules defined within a `ChildRules` block are applied to each item that passes the outer condition.

See [Collections](docs/collections.md) for more information.

---

## Key Features

  * **Native Fluent API**: A custom-built, type-safe API for defining validation rules, offering superior control and readability.
  * **Dynamic Validation**: Converts `[Required]`, `[EmailAddress]`, `[Range]`, and other annotations into runtime validation rules.
  * **Conditional Logic**: The `When(...)` and `Otherwise(...)` methods enable complex, conditional rule sets for sophisticated validation flows.
  * **Custom Rules**: The `Must(...)` method allows developers to easily embed custom predicate-based validation logic directly into the fluent chain.
  * **Localization**: Supports localized error messages using `.resx` or static resource classes with conventional key-based resolution (`Property_Attribute`).
  * **High Performance**: Leverages metadata caching to ensure no boilerplate code and minimal runtime overhead.
  * **DI Integration**: Seamlessly integrates with ASP.NET Core DI via `IFluentValidator<T>`.
  * **Complete Debug Support**: Includes full Source Link and step-through symbols for easy debugging.

-----

## Installation

Install via NuGet:

| Package | Version | Command |
|---|---|---|
| FluentAnnotationsValidator | 2.0.0-preview.2.3 | `dotnet add package FluentAnnotationsValidator --version 2.0.0-preview.2.3` |

-----

## Quickstart

See the [Quickstart](docs/configuration/fluent.md) guide for more.

-----

## Testing

The library includes a comprehensive test suite covering all major features, including:

  * Unit tests for all `[ValidationAttribute]` types.
  * Resolution of localized strings via `.resx` and static resources.
  * Validation of edge cases like multiple violations and fallback messages.
  * Full `Must(...)`, `When(...)`, and `Otherwise(...)` scenarios.

-----

## Documentation

More detailed documentation will be available in the `docs` directory with the full release.

-----

## License

Licensed under the MIT License.

-----

## Contributing

We welcome your feedback and contributions\! Feel free to submit pull requests or file issues to help improve the library.