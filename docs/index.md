## 📚 FluentAnnotationsValidator Documentation Index

Welcome to the official documentation for **FluentAnnotationsValidator**, a dynamic bridge between `System.ComponentModel.DataAnnotations` and FluentValidation with support for localization, performance caching, and ASP.NET Core integration.

Whether you're consuming the package or contributing to its internals, this documentation will help you understand its architecture, customize its behavior, and extend it for production-grade APIs.

---

### 📖 Table of Contents

| Topic | Description |
|-------|-------------|
| [Architecture Overview](architecture.md) | How core components like the validator, caching, and message resolver interact |
| [Localization Strategy](localization.md) | How error messages are resolved from `.resx` files or static resource classes |
| [Customization & Extensibility](customization.md) | How to override message resolution, disable caching, or inject custom logic |
| [Validation Flow](validation-flow.md) | Step-by-step walkthrough of how a DTO gets validated and errors are resolved |
| [Fluent Validation Configuration](configuration/fluent.md) | Domain-Specific Language allowing you to configure conditional validation per DTO and property |

## 📦 Version History

Looking for a specific release? Here are direct links to notable versions:

- [v2.0.0-preview.2.2](https://github.com/bigabdoul/fluent-annotations-validator/releases/tag/v2.0.0-preview.2.2) — Introduces a new, more expressive **fluent API**, marking a significant architectural shift.
- [v2.0.0-preview.2](https://github.com/bigabdoul/fluent-annotations-validator/releases/tag/v2.0.0-preview.2) — Marks a major architectural shift with the introduction of a new, highly expressive fluent API, and provides a more powerful and intuitive way to define complex and conditional validation rules, moving beyond a simple attribute-based approach
- [v2.0.0-preview1](https://github.com/bigabdoul/fluent-annotations-validator/releases/tag/v2.0.0-preview1) — **Major preview** release shifting to version 2, and introducing breaking changes: API surface and behavior may evolve in later stable builds
- [v1.1.0](https://github.com/bigabdoul/fluent-annotations-validator/releases/tag/v1.1.0) — Introduced fluent validation DSL, conditional configuration, and message/localization chaining, and framework-neutral structure
- [v1.0.6](https://github.com/bigabdoul/fluent-annotations-validator/releases/tag/v1.0.6) — Core annotations adapter with localization and DI support
- [v1.0.5](https://github.com/bigabdoul/fluent-annotations-validator/releases/tag/v1.0.5)
- [v1.0.4](https://github.com/bigabdoul/fluent-annotations-validator/releases/tag/v1.0.4)
- [v1.0.3](https://github.com/bigabdoul/fluent-annotations-validator/releases/tag/v1.0.3)
- [v1.0.2](https://github.com/bigabdoul/fluent-annotations-validator/releases/tag/v1.0.2)
- [v1.0.1](https://github.com/bigabdoul/fluent-annotations-validator/releases/tag/v1.0.1)
- [v1.0.0](https://github.com/bigabdoul/fluent-annotations-validator/releases/tag/v1.0.0)

For full changelogs, see [`CHANGELOG.md`](../CHANGELOG.md)
