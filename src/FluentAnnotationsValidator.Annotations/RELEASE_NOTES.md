## FluentAnnotationsValidator – Release Notes

### Version 2.0.0-rc.1.0.0 – Modular Architecture Release Candidate

**Release Date:** 2025-10-02

This release marks a major architectural evolution of FluentAnnotationsValidator, introducing a modular design that enhances clarity, reusability, and integration flexibility across .NET projects.

### Project Split: A New Modular Foundation

To better support standalone use cases and encourage clean separation of concerns, the original FluentAnnotationsValidator project has been split into five focused packages:

- **FluentAnnotationsValidator**  
  This layer integrates with DI containers, orchestrates the discovery and registration of custom attributes inheriting from `ValidationAttribute`, and contains `IFluentValidator<T>`, and `IValidationMessageResolver` implementations.

- **FluentAnnotationsValidator.Annotations**  
  Contains all custom validation attributes used in fluent rule composition. Designed to be lightweight and reusable across validation frameworks.

- **FluentAnnotationsValidator.Core**  
  Hosts the core abstractions, rule builders, and fluent APIs for defining and composing validation logic. This is the heart of the fluent validation experience.

- **FluentAnnotationsValidator.Runtime**  
  Provides runtime services for rule discovery, attribute parsing, localization, and validation execution. This layer powers dynamic validation pipelines and integrates with DI containers.

- **FluentAnnotationsValidator.AspNetCore**  
  Provides ASP.NET Core endpoint integration for `FluentAnnotationsValidator`, enabling automatic model validation via endpoint filters.

### Why Modular?

- **Reusability**: Use only what you need - attributes, core APIs, or runtime - without pulling in unnecessary dependencies.
- **Interoperability**: Plug into other validation frameworks or custom engines with minimal friction.
- **Testability**: Smaller, focused packages simplify unit testing and contributor onboarding.
- **NuGet Clarity**: Each package has a distinct purpose, making it easier to discover and adopt.

---

### Migration Notes

This release introduces **breaking changes** as part of the modular restructuring:

- **New namespaces** have been introduced:
  - `FluentAnnotationsValidator.Annotations` for custom attributes
  - `FluentAnnotationsValidator.Core` for fluent rule builders and abstractions
  - `FluentAnnotationsValidator.Runtime` for validation execution and localization

- **Legacy namespaces** from the original monolithic project have been removed or renamed. Any references to `FluentAnnotationsValidator` types must be updated to their new modular equivalents.

**Step-by-step migration guide**
1. Remove all references to `FluentAnnotationsValidator.Configuration`
2. Add the following packages and bring their corresponding namespaces into scope as required:
	- `FluentAnnotationsValidator`
	- `FluentAnnotationsValidator.Core`
	- `FluentAnnotationsValidator.Annotations`
	- `FluentAnnotationsValidator.Runtime`

- **Type and API relocation**:
  - New validation attributes like `MinimumAttribute` and `MaximumAttribute`, and older ones like `EmptyAttribute` or `NotEqualAttribute` now reside in `FluentValidator.Annotations`.
  - Fluent rule composition interfaces and extension methods are in `.Core`
  - Runtime services such as `FluentTypeValidatorRoot` are in `.Runtime`
  - `IFluentValidator<T>`, despite being declared in the `FluentAnnotationsValidator.Core` package and because of its central role in the validation pipeline, remains in the `FluentAnnotationsValidator` namespace.
  - The `FluentAnnotationsValidator.Results` namespace has become `FluentAnnotationsValidator.Core.Results`

- **Dependency updates required**:
  - Projects must reference the appropriate modular NuGet packages based on usage
  - Ensure your DI container is configured to register validators from `.Runtime`

- **Tests and tooling**:
  - Unit tests referencing internal types or reflection-based rule discovery may need to be updated to reflect new namespaces and type locations

---

## Maintainers

Maintained by the FluentAnnotationsValidator team. Feedback, contributions, and feature requests are welcome via GitHub issues and discussions.