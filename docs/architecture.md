---
title: Architecture Overview
breadcrumb: FluentAnnotationsValidator > Documentation > Architecture Overview
version: v2.0.0-preview.2
---

# 🏗️ Architecture Overview

### 1. **Core Design Philosophy**
FluentAnnotationsValidator is built around a **type-safe, override-safe validation engine** 
that dynamically translates `[ValidationAttribute]` annotations and fluent rule definitions 
into runtime validation logic. Although in its early development process, it represents an 
alternative to the FluentValidation by offering its own fluent DSL, conditional logic, and 
localization support.

---

### 2. **Key Components**

| Layer | Purpose |
|-------|--------|
| **Metadata** | Handles `[ValidationResource]` annotations and resource resolution for localized messages. |
| **Configuration** | Hosts the fluent DSL surface (`Rule`, `RuleFor`, `Must`, `When`, etc.), rule builders, and behavior options. |
| **Runtime.Validators** | Executes validation logic for both attribute-based and fluent-defined rules. Includes custom validators like `FluentLengthAttribute`, `EqualAttribute`, etc. |
| **Internals.Reflection** | Caches metadata and member info for performance. Powers dynamic rule resolution and override detection. |
| **Messages** | Manages localization, formatting, and fallback logic for error messages. Supports `.resx` and static resource classes. |
| **Results** | Defines validation result types and error structures returned during runtime validation. |
| **Extensions** | Provides service registration methods like `AddFluentAnnotations(...)` for ASP.NET Core integration. |

---

### 3. **Validation Flow**

1. **Model Discovery**  
   Types are scanned for `[ValidationAttribute]` annotations, `IFluentValidatable` implementations, or registered via `.For<T>()`.

2. **Rule Registration**  
   Rules are attached using either:
   - Static attributes (`[Required]`, `[EmailAddress]`, etc.)
   - Fluent DSL (`Rule(x => x.Password).Required().MinimumLength(6).MaximumLength(20)`, `RuleFor(x => x.Email).NotEmpty().EmailAddress()`)

3. **Conditional Logic**  
   Rules can be grouped under `When(...)` and `Otherwise(...)` blocks for dynamic evaluation.

4. **Localization**  
   Error messages are resolved using:
   - Conventional keys (`Property_Attribute`)
   - Explicit resource keys
   - Fallback messages

5. **Execution**  
   At runtime, `IFluentValidator<T>` validates the model, applying all applicable rules and returning structured results.

```plaintext
┌─────────────────────────────┐
│     1. Model Discovery      │
│  (Scan types, attributes)   │
└─────────────┬───────────────┘
              │
              ▼
┌─────────────────────────────┐
│     2. Rule Registration    │
│  (Attributes + Fluent DSL)  │
└─────────────┬───────────────┘
              │
              ▼
┌─────────────────────────────┐
│  3. Conditional Logic       │
│  (`When`/`Otherwise`)       │
└─────────────┬───────────────┘
              │
              ▼
┌─────────────────────────────┐
│     4. Localization         │
│  (Resolve error messages)   │
└─────────────┬───────────────┘
              │
              ▼
┌─────────────────────────────┐
│     5. Execution            │
│  (Run rules, return results)│
└─────────────────────────────┘
```
---

### 4. **Extensibility Points**

- `IFluentValidator<T>`: Custom validators can be injected and extended.
- `ConditionalValidationRule`: Supports per-rule predicates and override logic.
- `BeforeValidation(...)`: Allows pre-validation value transformation or initialization.
- `ValidationBehaviorOptions`: Centralized configuration for culture, resource types, and rule override behavior.

---

### 5. **Solution Structure**

```
src/
├── FluentAnnotationsValidator/
│   ├── Abstractions/
│   ├── Configuration/
│   ├── Extensions/
│   ├── Internals/Reflection/
│   ├── Messages/
│   ├── Metadata/
│   ├── Results/
│   └── Runtime/Validators/

tests/
├── FluentAnnotationsValidator.Tests/
│   ├── Configuration/
│   ├── Messages/
│   ├── Models/
│   ├── Resources/
│   ├── Results/
│   ├── Validators/
│   └── TestHelpers.cs
```
