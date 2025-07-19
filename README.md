# fluent-annotations-validator
A lightweight FluentValidation adapter that dynamically builds validators from ValidationAttributes defined in System.ComponentModel.DataAnnotations, including [Required], [EmailAddress], [StringLength], and custom error message logic.

## Purpose

A lightweight FluentValidation adapter that dynamically builds validators from `ValidationAttribute`s defined in `System.ComponentModel.DataAnnotations`, including `[Required]`, `[EmailAddress]`, `[StringLength]`, and custom error message logic.

---

## Key Features

- Reflection-based validator that respects `ErrorMessage`, `ErrorMessageResourceType`, and `FormatErrorMessage`
- Metadata caching for high-performance startup
- Plug-and-play `IValidator<T>` support via DI
- Localization-ready via `.resx` resources
- Fully compatible with ASP.NET Core Minimal APIs and MVC

---

## Project Structure

```
FluentAnnotationsValidator/
├── Validators/
│   └── DataAnnotationsValidator.cs
├── Internal/
│   ├── PropertyValidationInfo.cs
│   └── ValidationMetadataCache.cs
├── Extensions/
│   └── ServiceCollectionExtensions.cs
├── Resources/
│   └── ValidationMessages.resx (optional)
├── Tests/
│   └── FluentAnnotationsValidator.Tests.csproj
├── README.md
├── FluentAnnotationsValidator.csproj
```

---

## Usage Example

```csharp
builder.Services.AddFluentAnnotationsValidators();

// Registers IValidator<T> for any class with [ValidationAttribute] metadata
```

In your DTO:

```csharp
public class RegistrationDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [MinLength(6)]
    public string Password { get; set; }
}
```

And in your endpoint or controller:

```csharp
app.MapPost("/register", async (RegistrationDto dto, IValidator<RegistrationDto> validator) =>
{
    var result = await validator.ValidateAsync(dto);
    if (!result.IsValid) return Results.BadRequest(result.Errors);
    ...
});
```
