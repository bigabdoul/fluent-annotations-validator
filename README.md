## FluentAnnotationsValidator

A lightweight, dynamic bridge between `System.ComponentModel.DataAnnotations` and FluentValidation.

---

### ✨ Purpose

`FluentAnnotationsValidator` is a reflection-powered adapter that converts standard `[ValidationAttribute]` annotations into fluent validation rules at runtime. It supports localized error messaging, DI registration, and performance caching — making it a drop-in enhancement for any .NET API or ASP.NET Core backend.

---

### 🧠 Key Features

- Converts `[Required]`, `[EmailAddress]`, `[MinLength]`, `[StringLength]`, `[Range]`, and more to FluentValidation rules
- Resolves localized error messages from `.resx` or static resource classes
- Supports conventional message keys (`Property_Attribute`) and explicit `ErrorMessageResourceName`
- High-performance validation with caching and no boilerplate
- Seamless registration via DI for ASP.NET Core (`IValidator<T>`)
- Compatible with Minimal APIs, MVC, and Blazor

---

### 📦 Installation

Add via NuGet:

```bash
dotnet add package FluentAnnotationsValidator
```

To enable ASP.NET Core integration:

```bash
dotnet add package FluentAnnotationsValidator.AspNetCore
```

---

### 🚀 Quickstart

#### 1. Register validators (Minimal API or MVC)

```csharp
builder.Services.AddFluentAnnotationsValidators();
```

#### 2. Decorate your DTO using standard attributes

```csharp
[ValidationResource(typeof(ValidationMessages))]
public class RegistrationDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = default!;
}
```

#### 3. Resolve localized messages via convention:

```csharp
public static class ValidationMessages
{
    public const string Email_Required = "Email is required.";
    public const string Email_EmailAddress = "Email format is invalid.";
    public const string Password_Required = "Password is required.";
    public const string Password_MinLength = "Password must be at least {0} characters.";
}
```

#### 4. Validate in endpoint

```csharp
app.MapPost("/register", async (RegistrationDto dto, IValidator<RegistrationDto> validator) =>
{
    var result = await validator.ValidateAsync(dto);

    if (!result.IsValid)
        return Results.BadRequest(result.Errors);

    // Proceed with registration
});
```

---

### 🧪 Testing

Included in `FluentAnnotationsValidator.Tests`:

- Unit tests for all supported `ValidationAttribute` types
- Localized error resolution using `.resx` and static resource classes
- Edge cases like invalid formats, missing values, and multiple violations

---

### 📚 Project Layout

```
src/
├── FluentAnnotationsValidator/
│   ├── DataAnnotationsValidator.cs
│   ├── ValidationMetadataCache.cs
│   ├── PropertyValidationInfo.cs
│   ├── ValidationMessageResolver.cs
│   └── ValidationResourceAttribute.cs
├── FluentAnnotationsValidator.AspNetCore/
│   └── ServiceCollectionExtensions.cs
tests/
├── FluentAnnotationsValidator.Tests/
│   ├── Models/
│   ├── Validators/
│   └── Resources/
```

---

### 📄 License

Licensed under the MIT License.
