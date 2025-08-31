---
title: Customization & Extensibility
breadcrumb: FluentAnnotationsValidator > Documentation > Customization & Extensibility
version: v2.0.0-preview.2.3
---

# 🧩 Customization & Extensibility – Updated Extension Point

## IValidationMessageResolver

You can implement your own message resolution strategy by inheriting:

```csharp
public interface IValidationMessageResolver
{
    string? ResolveMessage(PropertyValidationInfo propertyInfo, ValidationAttribute attr);
}
```

By default, ValidationMessageResolver uses:

- propertyInfo.TargetModelType to locate the resource class
- Convention: PropertyName_AttributeType for message keys
- Fallback to [ErrorMessage] or default text

Custom resolvers can inject external providers, telemetry, or override formatting logic.

Register via DI:

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
