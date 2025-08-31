---
title: Validation Flow
breadcrumb: FluentAnnotationsValidator > Documentation > Validation Flow
version: v2.0.0-preview.2.3
---

# 🔄 Validation Flow

## A DTO's Journey to Validation

A validation request for a Data Transfer Object (DTO) follows a clear, two-stage process: first, **pre-validation** to prepare the data, and then **core validation** to enforce rules.

### From DTO to Result

1.  **Request received:** Your controller or API endpoint receives a DTO instance.
2.  **Validator injection:** An instance of `IFluentValidator<T>` is injected and begins the validation process.
3.  **Rule gathering:** The validator retrieves all registered rules for the DTO's type from the global registry.
4.  **Pre-validation runs:** The `PreValidationValueProviderDelegate` for a member executes, allowing you to clean or transform the data (e.g., trimming whitespace or formatting values) before validation begins.
5.  **Core validation:** Each rule is executed. If a validation fails, the validator resolves the appropriate error message.
6.  **Error aggregation:** For every validation failure, a `ValidationErrorResult` is created and added to a list.
7.  **Result returned:** The validator returns a `FluentValidationResult` containing a collection of `ValidationErrorResult` instances.
8.  **Business logic:** If the validation result indicates no errors, your application can proceed with its business logic.

### Validation Flow Diagram

Here is a descriptive text diagram illustrating the validation flow, from the moment a DTO is received to the final result. It visualizes the two main stages: pre-validation and core validation.

```text
                                        +---------------------------------------+
                                        |  (0) Starting Point: DTO is received  |
                                        |  in a controller or API endpoint,     |
                                        |  and IFluentValidator<T> is injected  |
                                        +---------------------------------------+
                                                           |
                                                           |
                                                           v
+------------------+                   +-----------------------------------+
|                  |                   |  (1) The IFluentValidator<T>      |
|  Rule Registry   |<------------------|  gathers all rules for the DTO's  |
|                  |                   |  type from the registry.          |
+------------------+                   +-----------------------------------+
                                                       |
                                                       |
                                                       v
                             +------------------------------------------------+
                             |             (2) Pre-Validation Check           |
                             +------------------------------------------------+
                                               |
                                               |
                              +----------------+---------------+
                              |                                |
                              v                                v
                [ Is there a BeforeValidation delegate? ]    [ No ]
                              |                                |
                              v                                v
                  [ Execute delegate and mutate DTO ]     [ Proceed ]
                              |                                |
                              +--------------------------------+
                                               |
                                               |
                                               v
                         +------------------------------------------------+
                         |             (3) Core Validation Loop           |
                         +------------------------------------------------+
                                               |
                                               |
                              +----------------+------------------+
                              |                                   |
                              v                                   v
                   [ Validation fails? ]                 [ Validation passes ]
                              |                                   |
                              v                                   v
              [ Resolve message & add error to list ]    [ Loop continues to the next rule ]
                              |                                   |
                              +-----------------------------------+
                                                |
                                                |
                                                v
                              +----------------------------------+
                              |  (4) The FluentValidationResult  |
                              |  is returned with all errors.    |
                              +----------------------------------+
```

---

## Supported Contexts

This validator is designed to integrate seamlessly into a wide variety of .NET application types and contexts.

* **Minimal APIs**
* **MVC Actions**
* **Blazor Components**
* **Custom Models** in API endpoints
* **Any .NET Core** application