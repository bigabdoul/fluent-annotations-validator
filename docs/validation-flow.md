---
title: Validation Flow
breadcrumb: [FluentAnnotationsValidator](../README.md) > [Documentation](index.md) > Validation Flow
version: v1.0.6
---

# 🔄 Validation Flow

## From DTO to Result

1. API endpoint or controller receives annotated DTO
2. Validator is injected via `IValidator<T>`
3. FluentAnnotationsValidator reflects on attributes
4. Rules are built dynamically
5. Messages are resolved (localized or static)
6. Result is returned as `ValidationResult`

## Supported Contexts

- Minimal APIs
- MVC actions
- Blazor components
- API endpoints with custom models
- .NET Core