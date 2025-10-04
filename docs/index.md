# FluentAnnotationsValidator Documentation

Welcome to the official documentation for **FluentAnnotationsValidator** - a modular, culture-aware validation framework that elegantly bridges `System.ComponentModel.DataAnnotations` and FluentValidation. It supports localization, caching, ASP.NET Core integration, and a fluent DSL for expressive rule configuration.

Whether you're integrating it into your application or contributing to its internals, this guide will help you understand its architecture, customize its behavior, and extend it for production-grade APIs.

---

## Quick Navigation

| Section | Description |
|--------|-------------|
| [Getting Started](getting-started.md) | Minimal setup with examples that grow in complexity |
| [Architecture Overview](architecture.md) | How validators, caching, and message resolution interact |
| [Localization Strategy](localization.md) | Using `.resx` files or static classes to localize error messages |
| [Collections](collections.md) | Validating nested collections and recursive structures |
| [Customization & Extensibility](customization.md) | Overriding logic, disabling caching, injecting services |
| [Validation Flow](validation-flow.md) | Step-by-step breakdown of how validation is executed |
| [Fluent Configuration](configuration/fluent.md) | Fluent DSL for conditional, expressive validation rules |

---

## Version History

Explore key releases and architectural milestones:

- [v2.0.0-rc.1.0.0](https://github.com/bigabdoul/fluent-annotations-validator/releases/tag/v2.0.0-rc.1.0.0) — First release candidate for v2 with modular architecture
- [v2.0.0-preview.2.2](https://github.com/bigabdoul/fluent-annotations-validator/releases/tag/v2.0.0-preview.2.2) — Introduced expressive fluent API
- [v2.0.0-preview.2](https://github.com/bigabdoul/fluent-annotations-validator/releases/tag/v2.0.0-preview.2) — Major shift to fluent-first validation with conditional logic
- [v2.0.0-preview1](https://github.com/bigabdoul/fluent-annotations-validator/releases/tag/v2.0.0-preview1) — Breaking changes and foundational redesign
- [v1.1.0](https://github.com/bigabdoul/fluent-annotations-validator/releases/tag/v1.1.0) — Fluent DSL, localization chaining, and framework-neutral structure
- [v1.0.6](https://github.com/bigabdoul/fluent-annotations-validator/releases/tag/v1.0.6) — DI support and core annotations adapter
- [v1.0.5](https://github.com/bigabdoul/fluent-annotations-validator/releases/tag/v1.0.5)
- [v1.0.4](https://github.com/bigabdoul/fluent-annotations-validator/releases/tag/v1.0.4)
- [v1.0.3](https://github.com/bigabdoul/fluent-annotations-validator/releases/tag/v1.0.3)
- [v1.0.2](https://github.com/bigabdoul/fluent-annotations-validator/releases/tag/v1.0.2)
- [v1.0.1](https://github.com/bigabdoul/fluent-annotations-validator/releases/tag/v1.0.1)
- [v1.0.0](https://github.com/bigabdoul/fluent-annotations-validator/releases/tag/v1.0.0)

---

## For Contributors

If you're contributing to the project, check out:

- [Contributor Guide](https://github.com/bigabdoul/fluent-annotations-validator/blob/main/CONTRIBUTING.md)
- [Release Notes](https://github.com/bigabdoul/fluent-annotations-validator/blob/main/CHANGELOG.md)
- [API Reference](api/index.md)

We welcome thoughtful contributions that improve clarity, extensibility, and developer experience.

---

Let’s build validation that’s elegant, extensible, and globally aware.
