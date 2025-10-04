# Getting Started with FluentAnnotationsValidator

**FluentAnnotationsValidator** is a modular validation framework that seamlessly connects `System.ComponentModel.DataAnnotations` with FluentValidation. It empowers developers to write expressive, culture-aware validation rules with support for caching, localization, and ASP.NET Core integration.

This guide introduces the framework progressively—from simple attribute-based validation to fluent configuration and runtime integration—so you can get productive quickly and scale confidently.

---

## Basic Setup

To begin, install the core package:

```bash
dotnet add package FluentAnnotationsValidator
```

Now define your data transfer objects (DTOs) using intuitive, declarative attributes:

```csharp
using FluentAnnotationsValidator.Annotations;

namespace MyProject;

public class UserDto
{
    [NotEmpty, MinLength(5), MaxLength(20)]
    public string Username { get; set; } = default!;

    [Required, Equal("admin")]
    public string Role { get; set; } = default!;

    [Minimum(18)]
    public int Age { get; set; }
}

public class LoginDto
{
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
}
```

These attributes are culture-aware and extensible, making them ideal for global applications.

Next, register the validation services:

```csharp
using FluentAnnotationsValidator;

// Requires a reference to the Microsoft.Extensions.DependencyInjection.Abstractions package.
var services = new ServiceCollection();

// Assuming this code runs in the same MyProject assembly.
services.AddFluentAnnotations();

// Otherwise, use:
// services.AddFluentAnnotations(targetAssembliesTypes: typeof(MyProject.UserDto));
```

Once registered, you can validate your DTOs like this:

```csharp
var provider = services.BuildServiceProvider();
var validator = provider.GetRequiredService<IFluentValidator<UserDto>>();
var result = validator.Validate(new UserDto { Username = "", Role = "user" });

if (!result.IsValid)
{
    foreach (var error in result.Errors)
        Console.WriteLine($"{error.PropertyName}: {error.ErrorMessage}");
}
```

This gives you immediate feedback with localized error messages and structured validation results.

---

## Fluent Configuration

For more advanced scenarios, FluentAnnotationsValidator offers a fluent DSL that allows conditional logic, chaining, and overrides—all within a strongly typed configuration block:

```csharp
services.AddFluentAnnotations(new ConfigurationOptions
{
    ConfigureValidatorRoot = root =>
    {
        using var userConfig = root.For<UserDto>();

        userConfig.RuleFor(x => x.Username)
            .NotEmpty()
            .MinLength(5)
            .MaxLength(20);

        userConfig.RuleFor(x => x.Role)
            .Equal("admin");

        using var loginConfig = root.For<LoginDto>();

        loginConfig.RuleFor(x => x.Username)
            .Required().WithMessage("Please enter your user name.")
            .EmailAddress().WithMessage("Enter a valid email address.");

        loginConfig.Rule(x => x.Password)
            .Required()
            .WithMessage("Password cannot be empty.");
    },
    TargetAssembliesTypes = [typeof(MyProject.UserDto)],
});
```

This approach is ideal when you need dynamic rule composition, custom messages, or integration with external services.

---

## ASP.NET Core Integration

To integrate validation directly into your ASP.NET Core pipeline, install the integration package:

```bash
dotnet add package FluentAnnotationsValidator.AspNetCore
```

Then configure it in your application startup:

```csharp
// In Program.cs or something similar
using FluentAnnotationsValidator;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFluentAnnotations(new()
{
    ConfigureValidatorRoot = root =>
    {
        // Fluent configuration goes here...
    }
});

var app = builder.Build();
```

You can now validate models implicitly or explicitly in your endpoints:

```csharp
// Map endpoints
var routes = app.MapGroup("/auth");

// Use either implicit validation:
routes.MapValidPost<UserDto>("/register", async (UserDto model) =>
{
    // If the code reaches here, validation was successful.
    // Do registration...

    return Results.Ok(model);
});

// Or explicit validation:
routes.MapPost("/register", async (UserDto model, IFluentValidator<UserDto> validator) =>
{
    var result = await validator.ValidateAsync(model);
    if (!result.IsValid)
    {
        return Results.BadRequest(result.Errors);
    }

    // Do registration...

    return Results.Ok();
});

routes.MapValidPost<LoginDto>("/login", async (LoginDto model, UserManager<ApplicationUser> userManager, ILogger<LoginDto> logger) =>
{
    // Validation passed.
    var user = await userManager.FindByEmailAsync(model.Username);
    if (user is null || !await userManager.CheckPasswordAsync(user, model.Password))
    {
        return Results.Unauthorized();
    }

    return Results.Ok(new { token = Guid.NewGuid().ToString() });
});
```

This integration ensures validation is enforced before your business logic runs, reducing boilerplate and improving reliability.

---

## Next Steps

- Explore the [Architecture Overview](architecture.md) to understand how validators, caching, and localization interact.
- Dive into [Customization](customization.md) to override behavior or inject services.
- Browse the [API Reference](api/index.md) for full type documentation.

FluentAnnotationsValidator is designed to be intuitive for consumers and empowering for contributors. Whether you're validating simple forms or building global APIs, it adapts to your needs with clarity and precision.
