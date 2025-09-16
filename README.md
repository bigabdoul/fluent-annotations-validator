# FluentAnnotationsValidator

[![NuGet - FluentAnnotationsValidator](https://img.shields.io/nuget/v/FluentAnnotationsValidator.svg)](https://www.nuget.org/packages/FluentAnnotationsValidator)
[![NuGet Publish](https://github.com/bigabdoul/fluent-annotations-validator/actions/workflows/nuget-publish.yml/badge.svg)](https://github.com/bigabdoul/fluent-annotations-validator/actions)
[![Build](https://github.com/bigabdoul/fluent-annotations-validator/actions/workflows/build.yml/badge.svg)](https://github.com/bigabdoul/fluent-annotations-validator/actions/workflows/build.yml)
[![Test](https://github.com/bigabdoul/fluent-annotations-validator/actions/workflows/test.yml/badge.svg)](https://github.com/bigabdoul/fluent-annotations-validator/actions/workflows/test.yml)
[![Source Link](https://img.shields.io/badge/SourceLink-enabled-brightgreen)](https://github.com/dotnet/sourcelink)

FluentAnnotationsValidator is a modern, fluent validation library that combines the declarative power of data annotations with the flexibility and readability of a fluent API. This library simplifies the process of defining complex validation rules for your C# models.

-----

## Purpose

`FluentAnnotationsValidator` is a reflection-powered adapter that dynamically converts standard `[ValidationAttribute]` annotations into powerful validation rules at runtime. This new version replaces the dependency on the `FluentValidation` package with a native, streamlined engine, providing a cleaner, more intuitive, and fully integrated experience.

-----

## ✨ New

### `RuleForEach` for Collection Validation

The `RuleForEach` method is a new addition that enables you to apply validation rules to each item within a collection. This is particularly useful for validating complex nested models, as it allows you to define rules for properties inside a list or array.

When combined with `When` and `Otherwise`, `RuleForEach` becomes a powerful tool for creating highly conditional validation logic on collections. The inner rules defined within a `ChildRules` block are applied to each item that passes the outer condition.

**Example**

```csharp
FluentTypeValidator<ConditionalTestDto> configurator;
var services = new ServiceCollection();

services.AddFluentAnnotations(new ConfigurationOptions
{
    ConfigureValidatorRoot = config => configurator = config.For<ConditionalTestDto>(),
    ExtraValidatableTypesFactory = () => [typeof(ConditionalTestDto)],
});

// Make the compiler happy by showing that 'configurator' is not null below.
ArgumentNullException.ThrowIfNull(configurator);

// Configure a rule that is applied only when the condition is false.
configurator.RuleForEach(x => x.Items)
    .When(x => x.Age >= 21, rules =>
    {
        rules.ChildRules(item => item.RuleFor(x => x.ItemName).NotEmpty());
        rules.Must(AlwaysPass).WithMessage("This rule should not run.");
    })
    .Otherwise(rules =>
    {
        rules.ChildRules(item =>
        {
            item.RuleFor(x => x.ItemName)
                .Required()
                .NotEmpty()
                .Must(StartWithItemA).WithMessage(x => $"{x.ItemName} failed validation.");

            item.RuleForEach(x => x.Products)
                .When(x => x.ItemName == "Item A but different", product =>
                {
                    product
                        .NotEmpty()
                        .ChildRules(prod =>
                        {
                            prod.RuleFor(p => p.ProductId)
                                .NotEmpty()
                                .ExactLength(36).WithMessage("Product Id must be exactly 36 chars long."); // Fail: Value has 32 chars

                            prod.RuleForEach(p => p.Orders)
                                .NotEmpty()
                                .ChildRules(order =>
                                {
                                    order.RuleFor(c => c.OrderId)
                                        .Required()
                                        .NotEmpty()
                                        .ExactLength(36).WithMessage("Order Id must be exactly 36 chars long.");

                                    order.RuleFor(c => c.ProductId)
                                        .Required()
                                        .NotEmpty()
                                        .ExactLength(36).WithMessage("Product Id for order must be exactly 36 chars long."); ;

                                    order.RuleFor(c => c.Quantity)
                                        .Range(1, 1000).WithMessage("Quantity must be between 1 and 1000.");
                                });
                        });
                });
        });
    });

configurator.Build();
```

### Retrieve an instance of IFluentValidator and validate a model.

```csharp
// This configuration has 13 validation errors.
var testDto = new ConditionalTestDto
{
    Age = 10, // Condition causes the 'Otherwise' branch to execute.
    Items =
    [
        /*Items[0]*/new() { ItemName = "Item A" }, // No Error @ index 0: ItemName starts with "Item A" 
        /*Items[1]*/new() { ItemName = "Item B" }, // Error 1 @ index 1: Must(StartWithItemA)
        /*Items[2]*/new() { ItemName = null! }, // Errors 2, 3, and 4 @ index 2: Required, NotEmpty, and Must(StartWithItemA)
        /*Items[3]*/new() { ItemName = string.Empty }, // Errors 5, 6 and 7 @ index 3: Required, NotEmpty, and Must(StartWithItemA)
        /*Items[4]*/new()
        {
            /*Items[4].*/ItemName = "Item A but different",
            /*Items[4].*/Products =
            [
                /*Items[4].Products[0]*/new()
                {
                    // Error 8: ExactLength must be 36 chars; is 32 chars long because ToString("n") strips away the dashes.
                    /*Items[4].Products[0].*/ProductId = Guid.NewGuid().ToString("n"),
                    /*Items[4].Products[0].*/Orders =
                    [
                        // Pass
                        /*Items[4].Products[0].Orders[0]*/new() { OrderId = Guid.NewGuid().ToString(), ProductId = Guid.NewGuid().ToString(), Quantity = 10 },
                        /*Items[4].Products[0].Orders[1]*/new()
                        {
                            /*Items[4].Products[0].Orders[1].*/OrderId = string.Empty, // Errors 9, 10, and 11: Required, NotEmpty, and ExactLength (36 chars) not met
                            /*Items[4].Products[0].Orders[1].*/ProductId = Guid.NewGuid().ToString("n"), // Error 12: ExactLength 32 chars, requires exactly 36
                            /*Items[4].Products[0].Orders[1].*/Quantity = 5
                        },
                        // Error 13: Range - Quantity not in range [1, 1000]
                        /*Items[4].Products[0].Orders[2]*/new() { OrderId = Guid.NewGuid().ToString(), ProductId = Guid.NewGuid().ToString(), Quantity = -20 },
                    ]
                }
            ]
        },
    ]
};
```

The `IFluentValidator<T>` service usually comes from the DI registration pipeline. For instance, in a controller action or a minimal API endpoint.

```csharp
var validator => services.BuildServiceProvider().GetRequiredService<IFluentValidator<ConditionalTestDto>>();

var result = validator.Validate(testDto);

// Do something with the result:
if (!result.IsValid)
{
    foreach (var err in result.Errors)
    {
        //...
    }
}
```

This example demonstrates how you can build a deeply nested validation structure. The `RuleForEach` on `Products` applies rules to each product within the `Items` collection, and a subsequent `RuleForEach` on `Orders` validates each order within a specific product. This allows for a very granular, clean, and powerful validation strategy.

---

## 🧠 Key Features

  * **Native Fluent API**: A custom-built, type-safe API for defining validation rules, offering superior control and readability.
  * **Dynamic Validation**: Converts `[Required]`, `[EmailAddress]`, `[Range]`, and other annotations into runtime validation rules.
  * **Conditional Logic**: The `When(...)` and `Otherwise(...)` methods enable complex, conditional rule sets for sophisticated validation flows.
  * **Custom Rules**: The `Must(...)` method allows developers to easily embed custom predicate-based validation logic directly into the fluent chain.
  * **Localization**: Supports localized error messages using `.resx` or static resource classes with conventional key-based resolution (`Property_Attribute`).
  * **High Performance**: Leverages metadata caching to ensure no boilerplate code and minimal runtime overhead.
  * **DI Integration**: Seamlessly integrates with ASP.NET Core DI via `IFluentValidator<T>`.
  * **Complete Debug Support**: Includes full Source Link and step-through symbols for easy debugging.

-----

## 📦 Installation

Install via NuGet:

| Package | Version | Command |
|---|---|---|
| FluentAnnotationsValidator | 2.0.0-preview.2.3 | `dotnet add package FluentAnnotationsValidator --version 2.0.0-preview.2.3` |

-----

## 🚀 Quickstart

### 1\. DTO Annotations

Create your Data Transfer Object (DTO) and decorate properties with standard `System.ComponentModel.DataAnnotations`.

```csharp
using FluentAnnotationsValidator.Metadata;
using System.ComponentModel.DataAnnotations;

public class BaseIdentityModel
{
    [Required]
    public virtual string Email { get; set; } = default!;

    [Required, StringLength(20, MinimumLength = 6)]
    public virtual string Password { get; set; } = default!;
}

[ValidationResource(typeof(FluentValidationMessages))]
public class RegisterModel : BaseIdentityModel
{
    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(50)]
    public string? FirstName { get; set; }

    [StringLength(50)]
    public string? LastName { get; set; }

    public DateTime? BirthDate { get; set; }
}

public class LoginModel : BaseIdentityModel
{
    public IList<string> Scopes { get; set; } = [];
}
```

### 2\. Localized Messages

Use a `.resx` file to define localized validation messages with conventional keys (`Property_Attribute`).

```csharp
public class FluentValidationMessages
{
    public static System.Globalization.CultureInfo? Culture { get; set; }
    public const string Email_Required = "Email is required.";
    public const string Email_NotEmpty = "Email is cannot be empty.";
    public const string Email_EmailAddress = "Email format is invalid.";
    public const string Password_Required = "Password is required.";
    public const string Password_StringLength = "The Password field must be a string with a minimum length of {0} and a maximum length of {1}.";
    public const string Password_Must = "Password must contain at least one digit.";
}
```

### 3\. Configuration

Use the `AddFluentAnnotations(...)` extension method in your `Startup.cs` or `Program.cs`.

```csharp
using FluentAnnotationsValidator.Extensions;
using FluentAnnotationsValidator.Configuration;

public static partial class FluentValidationUtils
{
    public static IServiceCollection ConfigureFluentAnnotations(this IServiceCollection services)
    {
        services.AddFluentAnnotations(
            // This factory makes the validation exclusively French
            localizerFactory: factory => new(SharedResourceType: typeof(FluentValidationMessages), SharedCulture: CultureInfo.GetCultureInfo("fr")),
            configure: config =>
            {
                // RegisterModel configuration
                var registrationConfig = config.For<RegisterModel>();

                // Add a preemptive rule that overrides any previous configuration for 'Email'
                registrationConfig.Rule(x => x.Email)
                    .Required()
                    .EmailAddress();

                // Add a non-preemptive rule for 'Email' that adds more rules to the property
                registrationConfig.RuleFor(x => x.Email)
                    .When(dto => dto.Email.EndsWith("@example.com"),
                        rule => rule.Must(email => email.Any(char.IsDigit))
                    );

                // Add a preemptive rule that does NOT override previous configuration for 'Password'
                registrationConfig.Rule(x => x.Password,
                    must: BeComplexPassword,
                    RuleDefinitionBehavior.Preserve);

                registrationConfig.RuleFor(x => x.BirthDate)
                    .When(x => x.BirthDate.HasValue, rule => rule.Must(BeAtLeast13));

                registrationConfig.Build();

                // LoginModel configuration
                var loginConfig = config.For<LoginModel>();

                // Non-preemptive rule that retains statically defined constraints (RequiredAttribute).
                // Add a rule to distinguish between admins, and other users. This scenario is:
                // "If the email does not contain the @ symbol, the Scopes collection must contain 'admin';
                // otherwise, Scopes must be empty or contain 'user'."
                loginConfig.RuleFor(x => x.Scopes)
                    .When(IsNotBlankInvalidEmail, scopes => scopes.Must(ContainAdminScopes))
                    .Otherwise(scopes => scopes.Must(BeEmptyOrContainUserScope));

                // This Must(...) method is an alias for When(...) to make the intent clearer.
                loginConfig.RuleFor(x => x.Email)
                    .Must(BeValidEmailAddressIfNotAdmin, rule => rule.EmailAddress());

                loginConfig.Build();
            },
            targetAssembliesTypes: [typeof(RegisterModel)]
        );
    }
    
    private static bool BeAtLeast13(DateTime? date) => DateTime.UtcNow.AddYears(-13) >= date;

    private static bool BeComplexPassword(string password)
    {
        // A regular expression that checks for a complex password.
        // (?=.*[a-z])   - Must contain at least one lowercase letter.
        // (?=.*[A-Z])   - Must contain at least one uppercase letter.
        // (?=.*\d)      - Must contain at least one digit.
        // (?=.*[!@#$%^&*()_+=\[{\]};:\"'<,>.?/|\-`~]) - Must contain at least one non-alphanumeric character.
        // .             - Matches any character (except newline).

        var passwordRegex = ComplexPasswordRegex();

        return passwordRegex.IsMatch(password);
    }

    private static bool IsNotBlankInvalidEmail(LoginModel m) => !string.IsNullOrWhiteSpace(m.Email) && !m.Email.Contains('@');

    private static bool ContainAdminScopes(HashSet<string> scopes) => scopes.Contains("admin") || scopes.Contains("superuser");

    private static bool BeEmptyOrContainUserScope(HashSet<string> scopes) => scopes.Count == 0 || scopes.Contains("user");

    private static bool BeValidEmailAddressIfNotAdmin(LoginModel user)
    {
        var email = user.Email;
        if (string.IsNullOrWhiteSpace(email)) return true; // invalid, enacts the EmailAddress() validator

        return
            // if it contains an @ symbol, then it must be a valid email address
            email.Contains('@') ||
            // non-administrators definitely need a valid email address
            !string.Equals(email, "admin", StringComparison.OrdinalIgnoreCase) && 
            !string.Equals(email, "superuser", StringComparison.OrdinalIgnoreCase) &&
            !user.Scopes.Contains("admin");
    }

    private static bool BeComplexPassword(string password)
    {
        // A regular expression that checks for a complex password.
        // (?=.*[a-z])   - Must contain at least one lowercase letter.
        // (?=.*[A-Z])   - Must contain at least one uppercase letter.
        // (?=.*\d)      - Must contain at least one digit.
        // (?=.*[!@#$%^&*()_+=\[{\]};:\"'<,>.?/|\-`~]) - Must contain at least one non-alphanumeric character.
        // .             - Matches any character (except newline).

        var passwordRegex = ComplexPasswordRegex();

        return passwordRegex.IsMatch(password);
    }

    [GeneratedRegex(@"(?:\+?224|00224)?[\s.-]?(?:\d{3}[\s.-]?\d{3}[\s.-]?\d{3}|\d{3}[\s.-]?\d{2}[\s.-]?\d{2}[\s.-]?\d{2})")]
    private static partial Regex GuineaPhoneNumberRegex();

    [GeneratedRegex("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^a-zA-Z\\d]).*$")]
    private static partial Regex ComplexPasswordRegex();
}
```

#### 3\.1 No `ValidationAttribute` annotation on model's member

To make sure the library handles the case where a model's members don't contain any 
`ValidationAttribute` annotation, the model must either implement the `IFluentValidatable`
marker interface, or pass the `extraValidatableTypes` parameter to the extension method
`IServiceCollection.AddFluentAnnotations(...)`.

Declare types:

```csharp
using FluentAnnotationsValidator.Abstractions;

public class Product : IFluentValidatable
{
    public string ProductId { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public bool IsPhysicalProduct { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
}

public class ProductOrder
{
    public string OrderId { get; set; } = default!;
    public string ProductId { get; set; } = default!;
    public int Quantity { get; set; }
}
```

Add validation rules:

```csharp
services.AddFluentAnnotations(
    configure: config =>
    {
        var productConfigurator = config.For<Product>();
        // Configure Product and Build
        productConfigurator.RuleFor(x => x.ShippingAddress)
            .When(x => x.IsPhysicalProduct, rule =>
            {
                // These rules are evaluated if IsPhysicalProduct is true
                rule.NotEmpty().MaximumLength(100);
            })
            .Otherwise(rule =>
            {
                // This rule will be evaluated if IsPhysicalProduct is false
                rule.Must(address => address == "N/A")
                    .WithMessage("The shipping address for non-physical products must be N/A.");
            });

        productConfigurator.Build();

        // Configure ProductOrder and Build
        var orderConfigurator = config.For<ProductOrder>();
        
        orderConfigurator.RuleFor(x => x.OrderId)
            .NotEmpty()
            .MinimumLength(8);

        orderConfigurator.Build();
    },
    extraValidatableTypes: () => [typeof(ProductOrder)],
    targetAssembliesTypes: [typeof(Product)]
);
```

#### 3\.2 Pre-Validation Value Providers

Pre-validation value providers is a new mechanism to modify or retrieve a member's value before validation.
This method is useful for data preparation, normalization, initialization, or fetching values from external sources.

Example:

```csharp
services.AddFluentAnnotations(
    configure: config =>
    {
        var productConfigurator = config.For<ProductModel>();

        // BeforeValidation(...) can be called in any order, but only
        // ONCE for this configurator, ProductModel, and ProductId.
        productConfigurator.RuleFor(x => x.ProductId)
            .BeforeValidation(EnsureProductIdInitialized)
            .Required()
            .NotEmpty()
            .ExactLength(36);

        productConfigurator.Build();
    }
);

// Makes sure productId is not blank.
static string? EnsureProductIdInitialized(ProductModel product, MemberInfo member, string? productId)
    => product.ProductId = string.IsNullOrWhiteSpace(productId) ? Guid.NewGuid().ToString() : productId;
```

### 4\. Runtime Validation

Inject `IFluentValidator<T>` into your services, controllers, or Minimal API endpoints to validate your DTOs.

```csharp
app.MapPost("/register", async (RegisterModel dto, IFluentValidator<RegisterModel> validator) =>
{
    var result = await validator.ValidateAsync(dto);
    if (!result.IsValid)
        return Results.BadRequest(result.Errors);

    // Proceed with registration
});
```

-----

## 🧪 Testing

The library includes a comprehensive test suite covering all major features, including:

  * Unit tests for all `[ValidationAttribute]` types.
  * Resolution of localized strings via `.resx` and static resources.
  * Validation of edge cases like multiple violations and fallback messages.
  * Full `Must(...)`, `When(...)`, and `Otherwise(...)` scenarios.

-----

## 🗂️ Solution Structure

| Project | Purpose |
|---|---|
| `FluentAnnotationsValidator` | The core validation engine, new fluent DSL, and extensibility points. |
| `FluentAnnotationsValidator.Tests` | A complete test suite covering all configuration, resolution, and validation flows. |

-----

## 📘 Documentation

More detailed documentation will be available in the `docs` directory with the full release.

-----

## 📄 License

Licensed under the MIT License.

-----

## 🤝 Contributing

We welcome your feedback and contributions\! Feel free to submit pull requests or file issues to help improve the library.