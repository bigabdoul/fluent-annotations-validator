---
title: Localization Strategy
breadcrumb: FluentAnnotationsValidator > Documentation > Localization Strategy
version: v1.0.6
---

# 🧠 Localization Strategy

## Supported Resource Models

- Static `.resx` files with auto-generated `.Designer.cs`
- Static resource classes with constant string fields

## Message Resolution Flow

1. Match `Property_Attribute` key (e.g. `Email_Required`)
2. Fallback to `[ErrorMessageResourceName]` if defined
3. Use `[ErrorMessage]` if set
4. Default to system message if nothing is resolved

## Example

```csharp
[Required(ErrorMessageResourceName = "CustomEmailRequired")]
public string Email { get; set; }

public static class ValidationMessages
{
    public const string CustomEmailRequired = "Please provide your email.";
}
```
