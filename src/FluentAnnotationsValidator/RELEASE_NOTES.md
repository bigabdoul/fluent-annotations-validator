## FluentAnnotationsValidator v2.0.0-preview.2.2

**Release date:** 2025-08-30

This release, `v2.0.0-preview.2.2`, marks a major architectural shift with the introduction of a new, highly expressive **fluent API**. This new API provides a more powerful and intuitive way to define complex and conditional validation rules, moving beyond a simple attribute-based approach.

#### Key Highlights

#### Conditional and Custom Validation
* **`When` and `Otherwise`**: You can now define `if/else` logic within your validation rules. This allows for complex validation scenarios where rules are only applied when a specific condition is met.
* **`Must`**: Integrate custom, predicate-based validation logic directly into your fluent chain for highly specific business rules that go beyond standard data annotations.

#### Pre-Validation Value Providers
A brand-new mechanism has been introduced to give you control over a member's value *before* validation begins. This is perfect for data normalization, populating default values, or preparing data from external sources.

#### Enhanced Fluent API
* **`Rule(...)`**: This method now preemptively replaces existing rules for a member, making it the perfect tool for explicitly overriding a previous configuration.
* **`RuleFor(...)`**: This new method provides a non-destructive way to add rules to a member, allowing you to chain validation logic without overwriting previous rules.

#### Core Changes & Utilities
* **Dependency Removal**: The dependency on the `FluentValidation` package has been removed entirely. All references to `IValidator<T>` should be updated to `IFluentValidator<T>`.
* **MemberInfo Extensions**: New utility methods (`GetValue`, `SetValue`, and `TrySetValue`) have been added to simplify dynamic access to object properties and fields using reflection.

This version lays the groundwork for a more flexible, powerful, and maintainable validation framework.