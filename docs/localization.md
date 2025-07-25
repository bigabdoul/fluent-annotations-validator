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

1. **Explicit Resource Name**  
   If `ErrorMessageResourceName` property is provided:
   - Use `ErrorMessageResourceType` if set
   - Otherwise, fall back to the model’s `[ValidationResource]` attribute
   - Retrieve static property by name and format

2. **Conventional Key (Property_Attribute)**  
   Construct key from property name and attribute type  
   e.g. `Email_Required` → `ValidationMessages.Email_Required`

3. **Inline Message or Fallback**  
   If `ErrorMessage` is set, format and return it  
   Else, fallback to `"Invalid value for {Property}"`

## Technical Note

The `PropertyValidationInfo` now includes a new field:

```csharp
public Type TargetModelType { get; set; }
```

This enables resolution logic to anchor back to the declaring DTO for 
resource scanning and convention fallback.

## Example

```csharp
[Required(ErrorMessageResourceName = "CustomEmailRequired")]
public string Email { get; set; }

public static class ValidationMessages
{
    public const string CustomEmailRequired = "Please provide your email.";
}
```
