---
title: Architecture Overview
breadcrumb: FluentAnnotationsValidator > Documentation > Architecture Overview
version: v1.0.6
---

# 🧠 Architecture Overview

This guide explores the internal structure of FluentAnnotationsValidator, explaining how standard `[ValidationAttribute]` annotations are parsed, cached, and translated into FluentValidation rules.

> Documentation for **FluentAnnotationsValidator v1.0.6**
---

## ⚙️ Core Components

### `DataAnnotationsValidator`
Main entry point. Uses reflection to scan DTOs and build `AbstractValidator<T>` instances dynamically.

### `ValidationMetadataCache`
Caches metadata like which attributes apply to each property. Reduces repeated reflection and boosts performance.

### `PropertyValidationInfo`
Represents individual property metadata, including resource references and validation rules.

### 🗣️ `ValidationMessageResolver`  
Generates error messages by:
- Scanning `.resx` or static resource classes
- Matching message keys like `Email_Required`
- Supporting fallback to `[ErrorMessage]` and system defaults

---

## 🧱 Architecture Diagram

```plaintext
           ┌────────────────────────┐
           │    RegistrationDto     │
           └─────────┬──────────────┘
                     │
                     ▼
        ┌────────────────────────────┐
        │ DataAnnotationsValidator   │
        └─────────┬──────────────────┘
                  │
     ┌────────────┴────────────┐
     ▼                         ▼
ValidationMetadataCache   ValidationMessageResolver
     │                         │
     ▼                         ▼
FluentValidation Rules     Localized Messages
```

---

## 🌐 ASP.NET Core Integration

Via:

```csharp
builder.Services.AddFluentAnnotationsValidators();
```

Scans all registered DTOs and wires up IValidator<T> into DI.

