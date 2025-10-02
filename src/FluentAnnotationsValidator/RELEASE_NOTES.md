## FluentAnnotationsValidator v2.0.0-rc.1.0.0

**Release date:** 2025-10-02

This preview release introduces powerful new capabilities for collection validation, enhanced conditional logic, and improved developer ergonomics. It continues the evolution toward a fully native, annotation-driven validation engine—no external dependencies required.

### New Features

- **RuleForEach Support**:  
  Apply validation rules to each item in a collection with full support for conditional logic via `When(...)` and `Otherwise(...)`. Nested `ChildRules` allow for deep, expressive validation of complex DTO hierarchies.

- **Pre-Validation Value Providers**:  
  Introduced `BeforeValidation(...)` hooks to normalize or initialize values before validation begins. Ideal for scenarios like auto-generating GUIDs or sanitizing input.

- **Dynamic Rule Composition**:  
  Validators now support runtime rule injection via `.RuleFor(...)` and `.Build()`, enabling flexible, context-aware validation logic without modifying DTOs.

- **Inheritance-Aware Validation**:  
  Validation rules defined for base types (e.g., `TestRegistrationDto`) are automatically respected by derived types (e.g., `InheritRulesRegistrationDto`), including both static and dynamic rules.

- **ExactLength Rule Support**:  
  Introduced `.ExactLength(n)` for precise string length enforcement, with customizable error messages using format placeholders.

- **Async Validation for Inherited Rules**:  
  Full support for asynchronous validation flows, even when rules are inherited or composed dynamically. This ensures consistent behavior across sync and async pipelines.

- **Multi-Error Aggregation**:  
  Properties can now accumulate multiple validation errors from different rule sources (e.g., `[Required]`, `.NotEmpty()`, `.Must(...)`), improving diagnostic clarity.

### Improvements

- Refined async unit testing for inheritance scenarios.
- Improved test workflow alignment with build artifacts.
- Updated documentation and examples for advanced validation flows.
- Enhanced support for models without `[ValidationAttribute]` via `IFluentValidatable` or `extraValidatableTypes`.

### Testing & Debugging

- Expanded test coverage for:
  - Deeply nested validation chains.
  - Conditional branches with multiple violations.
  - Localization fallback and override behavior.
- Full Source Link and step-through debugging support.

### Installation

```bash
dotnet add package FluentAnnotationsValidator --version 2.0.0-preview.2.3
```

---

This release sets the stage for a stable v2.0.0 by refining the fluent DSL and empowering developers to build scalable, contributor-friendly validation flows.
