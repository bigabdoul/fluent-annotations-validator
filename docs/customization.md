---
title: Customization & Extensibility
breadcrumb: FluentAnnotationsValidator > Documentation > Customization & Extensibility
version: v1.0.6
---

# 🧩 Customization & Extensibility

## Override Message Resolution

Implement `IValidationMessageResolver` and register via DI:

```csharp
services.AddSingleton<IValidationMessageResolver, MyCustomResolver>();
```

## Control Validator Caching

By default, validators are cached per DTO type. To disable:

```csharp
services.Configure<ValidatorOptions>(options =>
{
    options.EnableCaching = false;
});
```

## Add Custom Rules
Extend DataAnnotationsValidator with your own attributes:

```csharp
public class MyCustomAttribute : ValidationAttribute
{
    // Your logic here
}
```
